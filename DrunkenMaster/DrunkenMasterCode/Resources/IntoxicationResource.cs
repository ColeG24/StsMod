using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Commands;
using DrunkenMaster.DrunkenMasterCode.Tips;
using BaseLib.Abstracts;
using BaseLib.BaseLibScenes;
using BaseLib.Patches.UI;
using DrunkenMaster.DrunkenMasterCode.Ui;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace DrunkenMaster.DrunkenMasterCode.Resources;

/// <summary>
/// Spec §6. Intoxication as a first-class per-combat resource (BaseLib CustomResource) instead of a
/// power icon. Shown as a dial above the draw pile. Starts each combat at 3; decays 0 / 1 / 2 per turn while Sober / Tipsy / Drunk.
///
/// Discovered and registered by BaseLib automatically (parameterless ctor). One instance per
/// PlayerCombatState; use the static helpers from cards/relics.
/// </summary>
public class IntoxicationResource() : CustomResource(ResourceId)
{
    /// <summary>Powers that add Intoxication every turn (Bar Tab). Lets the dial forecast next turn.</summary>
    public interface IPerTurnSource
    {
        int IntoxicationPerTurn(Player player);
        /// <summary>Visual pulse when the per-turn amount is applied (PowerModel.Flash is protected).</summary>
        void FlashPerTurn();
        /// <summary>Runs after the turn-start change has landed. One-shot sources (Nightcap) remove themselves here.</summary>
        Task AfterTurnStartApplied(PlayerChoiceContext choiceContext) => Task.CompletedTask;
    }

    /// <summary>Every combat opens at 3 Intoxication, the top of Sober (2026-09-15; was 0).</summary>
    public const int StartingIntoxication = 3;

    /// <summary>
    /// Turn-start decay depends on the band you wake up in (2026-09-15; was a flat 1): Sober holds, Tipsy loses 1,
    /// Drunk loses 2. Sober is the resting state and 3 is where the dial settles when nothing pushes it.
    /// </summary>
    public static int DecayFor(Band band) => band switch
    {
        Band.Sober => 0,
        Band.Tipsy => 1,
        _ => 2
    };

    private bool _started;

    /// <summary>
    /// First turn of combat only: put the dial at <see cref="StartingIntoxication"/> before the turn-start change and
    /// the hand draw, so the opening hand already sees the real amount. Called from DrunkenMasterBands.AfterEnergyReset.
    /// </summary>
    public static async Task EnsureStarted(PlayerChoiceContext choiceContext, Player player)
    {
        var res = Get(player);
        if (res == null || res._started) return;
        res._started = true;
        if (res.Amount < StartingIntoxication) await GainAsync(choiceContext, player, StartingIntoxication - res.Amount);
    }

    /// <summary>
    /// The amount after a turn start from <paramref name="current"/>: decay and every per-turn source
    /// are summed and applied as ONE change, so a Bar Tab at the edge of a band never dips out of it
    /// and back in (which would re-roll the hand and re-trigger band-up powers every turn).
    /// </summary>
    private static int NextFrom(int current, Player player)
    {
        int next = current - DecayFor(BandFor(current));
        var creature = player.Creature;
        if (creature != null)
        {
            foreach (var source in creature.Powers.OfType<IPerTurnSource>())
                next += source.IntoxicationPerTurn(player);
        }
        return Math.Clamp(next, 0, Max);
    }

    /// <summary>What the dial will read after the owner's next turn start (Blackout resets to 0 first).</summary>
    public static (int projected, int delta) ProjectNextTurn(Player player)
    {
        int current = AmountOf(player);
        int next = NextFrom(current >= BlackoutAt ? 0 : current, player);
        return (next, next - current);
    }

    /// <summary>
    /// Apply the turn-start change once. Called from <see cref="DrunkenMasterBands"/> after the energy
    /// reset and before the hand is drawn, so drawn cards see the band you are actually in this turn.
    /// </summary>
    public static async Task ApplyTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        var res = Get(player);
        if (res == null) return;
        int current = res.Amount;
        int next = NextFrom(current, player);
        if (player.Creature != null)
        {
            foreach (var source in player.Creature.Powers.OfType<IPerTurnSource>())
                if (source.IntoxicationPerTurn(player) != 0) source.FlashPerTurn();
        }
        if (next > current) await GainAsync(choiceContext, player, next - current);
        else if (next < current) Lose(player, current - next);
        await res.FlushBandPowers(choiceContext);
        if (next < current) await NotifyLost(choiceContext, player, current - next);
        if (player.Creature != null)
        {
            foreach (var source in player.Creature.Powers.OfType<IPerTurnSource>().ToList())
                await source.AfterTurnStartApplied(choiceContext);
        }
    }

    /// <summary>Powers that want to act when the owner's band goes up (see <see cref="GainAsync"/>).</summary>
    public interface IBandRaisedListener
    {
        Task OnBandRaised(PlayerChoiceContext choiceContext, Band from, Band to);
    }

    /// <summary>
    /// Powers that react to the owner losing Intoxication (Walk It Off). Fired once per loss event with the amount
    /// actually lost: the turn-start decay, an Intoxication cost being paid, and the Blackout reset.
    /// </summary>
    public interface ILossListener
    {
        Task OnIntoxicationLost(PlayerChoiceContext choiceContext, int amount);
    }

    /// <summary>Tell the owner's <see cref="ILossListener"/> powers that <paramref name="amount"/> Intoxication was just lost.</summary>
    public static async Task NotifyLost(PlayerChoiceContext choiceContext, Player player, int amount)
    {
        if (amount <= 0 || player.Creature is not { IsDead: false } creature) return;
        foreach (var listener in creature.Powers.OfType<ILossListener>().ToList())
        {
            if (creature.IsDead) return;
            await listener.OnIntoxicationLost(choiceContext, amount);
        }
    }

    /// <summary>
    /// Powers that react to the owner gaining Intoxication (Walk It Off since 2026-09-17). Fired once per gain event from
    /// <see cref="GainAsync"/> with the amount actually gained, so a gain swallowed by the cap at 12 fires nothing, and
    /// the netted turn-start change (decay plus Bar Tab) is one event, not two.
    /// </summary>
    public interface IGainListener
    {
        Task OnIntoxicationGained(PlayerChoiceContext choiceContext, int amount);
    }

    /// <summary>How many times the owner has Blacked Out this combat (Rude Awakening). Combat-scoped like the resource itself.</summary>
    public int Blackouts { get; private set; }
    public void RecordBlackout() => Blackouts++;

    public const string ResourceId = "DRUNKENMASTER-INTOXICATION";

    public const int TipsyMin = 4;
    public const int DrunkMin = 8;
    public const int BlackoutAt = 12;
    public const int Max = BlackoutAt;

    public enum Band { Sober, Tipsy, Drunk, Blackout }

    public static Band BandFor(int amount) => amount switch
    {
        >= BlackoutAt => Band.Blackout,
        >= DrunkMin => Band.Drunk,
        >= TipsyMin => Band.Tipsy,
        _ => Band.Sober
    };

    public static Color BandColor(Band band) => band switch
    {
        Band.Sober => new Color("7fb8d8"),
        Band.Tipsy => new Color("efc851"),
        Band.Drunk => new Color("f08a3c"),
        _ => new Color("ff5555")
    };

    public Band CurrentBand => BandFor(Amount);

    public static string BandName(Band band) => band switch
    {
        Band.Sober => "Sober",
        Band.Tipsy => "Tipsy",
        Band.Drunk => "Drunk",
        _ => "Blackout"
    };

    /// <summary>Fired whenever the amount crosses a band boundary: (from, to).</summary>
    public event Action<Band, Band>? BandChanged;

    /// <summary>
    /// Every write goes through here (Gain / Lose / Spend / decay / Blackout reset), so this is the one
    /// place that notices a band transition. Entering Drunk re-rolls the costs of the cards already in
    /// hand; every transition announces itself on screen.
    /// </summary>
    public override int Amount
    {
        get => base.Amount;
        set
        {
            var from = BandFor(base.Amount);
            base.Amount = value;
            var to = BandFor(base.Amount);
            if (from != to) OnBandChanged(from, to);
        }
    }

    private void OnBandChanged(Band from, Band to)
    {
        var owner = Owner;
        if (owner != null && from < Band.Drunk && to >= Band.Drunk) RandomizeHandCosts(owner);
        if (owner != null && from >= Band.Drunk && to < Band.Drunk) ResetRandomizedCosts();
        BandChanged?.Invoke(from, to);
        if (owner != null && !TestMode.IsOn) NBandBanner.Show(owner, from, to);
    }

    /// <summary>Cards whose cost was re-rolled this turn, so sobering up can put them back.</summary>
    private readonly HashSet<CardModel> _randomizedThisTurn = [];

    /// <summary>
    /// Drunk: give a card a random cost 0–3 for THIS TURN only (2026-09-12; was until played). Poised cards are skipped. The game
    /// clears the modifier at end of turn; sobering up below Drunk clears it early.
    /// </summary>
    public static void RandomizeCost(Player player, CardModel card)
    {
        if (card.EnergyCost.Canonical < 0) return;   // X-cost / no cost
        if (card.Keywords.Contains(DrunkenMasterKeywords.Poised)) return;   // Intoxication-cost cards (2026-09-14)
        int cost = player.RunState.Rng.CombatEnergyCosts.NextInt(4);
        card.EnergyCost.SetThisTurn(cost);
        Get(player)?._randomizedThisTurn.Add(card);
        NCard.FindOnTable(card)?.PlayRandomizeCostAnim();
    }

    /// <summary>Back below Drunk: drop this turn's re-rolls so the cards read their real cost again.</summary>
    private void ResetRandomizedCosts()
    {
        foreach (var card in _randomizedThisTurn.ToList())
        {
            if (!card.EnergyCost.EndOfTurnCleanup()) continue;
            card.InvokeEnergyCostChanged();
            NCard.FindOnTable(card)?.PlayRandomizeCostAnim();
        }
        _randomizedThisTurn.Clear();
    }

    private static void RandomizeHandCosts(Player player)
    {
        var hand = player.PlayerCombatState?.Hand;
        if (hand == null) return;
        foreach (var card in hand.Cards.ToList()) RandomizeCost(player, card);
    }

    /// <summary>
    /// Band bonuses as real powers (2026-09-14), sized per band (2026-09-15). The numbers are the band's TOTAL, not a
    /// step on top of the band below: Tipsy is 1 Strength and 1 Dexterity, Drunk (and Blackout, until it resolves) is
    /// 2 Strength and -1 Dexterity. Sober grants nothing.
    /// </summary>
    public const int TipsyStrength = 1;
    public const int TipsyDexterity = 1;
    public const int DrunkStrength = 2;
    public const int DrunkDexterity = -1;
    public const int BlackoutCardsPlayed = 3;

    /// <summary>Relics that add to a band's Strength / Dexterity (Champion's Tankard: +1 / +1 while Tipsy or Drunk).</summary>
    public interface IBandBonus
    {
        int ExtraBandStrength(Player player, Band band);
        int ExtraBandDexterity(Player player, Band band);
    }

    private static (int str, int dex) BaseBandPowers(Band band) => band switch
    {
        Band.Sober => (0, 0),
        Band.Tipsy => (TipsyStrength, TipsyDexterity),
        _ => (DrunkStrength, DrunkDexterity)
    };

    /// <summary>The Strength and Dexterity <paramref name="band"/> grants this player, relics included.</summary>
    public static (int str, int dex) BandPowersFor(Player player, Band band)
    {
        var (str, dex) = BaseBandPowers(band);
        if (band == Band.Sober) return (0, 0);
        foreach (var relic in player.Relics.OfType<IBandBonus>())
        {
            str += relic.ExtraBandStrength(player, band);
            dex += relic.ExtraBandDexterity(player, band);
        }
        return (str, dex);
    }

    /// <summary>What the bands have granted so far, so a band change applies exactly the difference.</summary>
    private int _grantedStrength, _grantedDexterity;

    /// <summary>
    /// Band transitions are noticed in the sync Amount setter, but granting a power is a command. Every async write
    /// path (GainAsync, ApplyTurnStart, Spend, the Blackout reset) calls this straight after: it compares what the
    /// current band should grant with what has been granted and applies the delta via PowerCmd, so the powers land in
    /// the action stream on every machine. Removal is a plain negative apply like Flex: Strength stolen in between can
    /// leave you below where you started, which is the base game's own precedent.
    /// </summary>
    public async Task FlushBandPowers(PlayerChoiceContext choiceContext)
    {
        var owner = Owner;
        var creature = owner?.Creature;
        if (owner == null || creature == null || creature.IsDead) return;
        var (str, dex) = BandPowersFor(owner, CurrentBand);
        int dStr = str - _grantedStrength, dDex = dex - _grantedDexterity;
        if (dStr == 0 && dDex == 0) return;
        _grantedStrength = str; _grantedDexterity = dex;
        if (dStr != 0) await PowerCmd.Apply<StrengthPower>(choiceContext, creature, dStr, creature, null);
        if (dDex != 0) await PowerCmd.Apply<DexterityPower>(choiceContext, creature, dDex, creature, null);
    }

    /// <summary>
    /// Cards that cost Intoxication spend through here. The band powers are NOT flushed here (2026-09-18): the cost is
    /// paid before OnPlay, so Hurl at 4 previewed 15 with Tipsy's Strength and then hit for 14 once the spend dropped
    /// you to Sober. The Strength / Dexterity of the band you played the card from now stay until the card has
    /// resolved; <see cref="DrunkenMasterBands.AfterCardPlayed"/> flushes them. The dial and banner still move at
    /// once, and loss listeners hear about what was actually spent (X costs spend a different amount than asked).
    /// </summary>
    public override async Task<bool> Spend<T>(ICombatState combatState, AbstractModel? spender, int amount, bool optional)
    {
        int before = Amount;
        bool ok = await base.Spend<T>(combatState, spender, amount, optional);
        var owner = Owner;
        if (owner != null && Amount < before) await NotifyLost(new ThrowingPlayerChoiceContext(), owner, before - Amount);
        return ok;
    }

    public override Color MainColor => new("8a4b12");
    public override string IconPath => MainFile.ResPath + "/images/ui/intoxication.png";

    /// <summary>X-cost cards spend everything; require at least 1 so they can't be played "for free" at 0.</summary>
    public override bool CanAfford(CardModel card, int cost)
    {
        if (CustomResources<IntoxicationResource>.Cost(card)?.CostsX == true) return Amount >= 1;
        return Amount >= cost;
    }

    public override bool ShouldShowDisplay() => Owner?.Character is Character.DrunkenMaster;

    /// <summary>
    /// Decay is NOT applied here. The decay and every per-turn gain are netted into one change by
    /// <see cref="ApplyTurnStart"/> (see the note on <see cref="NextFrom"/>).
    /// </summary>
    public override void StartOfTurnReset(PlayerCombatState playerCombatState, ICombatState combatState)
    {
        _randomizedThisTurn.Clear();   // the game already dropped last turn's cost modifiers
    }

    /// <summary>
    /// The dial is owned by <see cref="NDrunkenMasterHud"/> (registered from MainFile). This only wires
    /// the cost badge shown on cards that cost Intoxication.
    /// </summary>
    public override void RegisterResourceVisuals<T>()
    {
        ValidateType<T>();
        var iconPath = IconPath!;
        var mainColor = MainColor;
        var getDisplay = ExtraCardUi.RegisterCreateCardUiElement(ExtraCardUi.CardUiPositioning.AroundCost,
            _ => new NAdditionalCostDisplay(ResourceId, iconPath, mainColor),
            (card, model, display) =>
            {
                if (model == null) return false;
                var cost = CustomResources<T>.Cost(model);
                if (cost == null) return false;
                display.UpdateCostVisual(card, cost, PileType.None);
                return true;
            });
        CustomResources<T>.UpdateCostVisuals += (card, cost, pileType) => getDisplay(card).UpdateCostVisual(card, cost, pileType);
    }

    // ----- static helpers for cards / relics / potions -----

    public static IntoxicationResource? Get(Player player)
    {
        var state = player.PlayerCombatState;
        return state == null ? null : CustomResources<IntoxicationResource>.Get(state);
    }

    public static int AmountOf(Player player) => Get(player)?.Amount ?? 0;

    /// <summary>Gain Intoxication, clamped to <see cref="Max"/>.</summary>
    public static void Gain(Player player, int amount)
    {
        var res = Get(player);
        if (res == null || amount <= 0) return;
        res.Amount = Math.Min(Max, res.Amount + amount);
    }

    /// <summary>
    /// Gain, then let the owner's powers react if a band was crossed upward (Dutch Courage). Use this
    /// from any async card/potion/power; the sync <see cref="Gain"/> stays for callers without a context.
    /// </summary>
    public static async Task GainAsync(PlayerChoiceContext choiceContext, Player player, int amount)
    {
        var res = Get(player);
        if (res == null || amount <= 0) return;
        var from = res.CurrentBand;
        int before = res.Amount;
        Gain(player, amount);
        var to = res.CurrentBand;
        await res.FlushBandPowers(choiceContext);
        if (player.Creature is not { IsDead: false } creature) return;
        if (res.Amount > before)
        {
            foreach (var listener in creature.Powers.OfType<IGainListener>().ToList())
            {
                if (creature.IsDead) return;
                await listener.OnIntoxicationGained(choiceContext, res.Amount - before);
            }
        }
        if (to <= from) return;
        foreach (var listener in creature.Powers.OfType<IBandRaisedListener>().ToList())
        {
            await listener.OnBandRaised(choiceContext, from, to);
        }
    }

    public static void Lose(Player player, int amount)
    {
        var res = Get(player);
        if (res == null || amount <= 0) return;
        res.Amount = Math.Max(0, res.Amount - amount);
    }

    /// <summary>The short Intoxication tip: what it is and the four band names. Cards add <see cref="BandTip"/> for the band they check.</summary>
    public static IHoverTip Tip => new HoverTip(
        new LocString("static_hover_tips", $"{ResourceId}.title"),
        new LocString("static_hover_tips", $"{ResourceId}.description"));

    public static IHoverTip BandTip(Band band) => HoverTipFactory.Static(band switch
    {
        Band.Sober => DrunkenMasterTips.Sober,
        Band.Tipsy => DrunkenMasterTips.Tipsy,
        Band.Drunk => DrunkenMasterTips.Drunk,
        _ => DrunkenMasterTips.Blackout
    });

    /// <summary>What the dial shows on hover: the resource tip followed by every band.</summary>
    public static IEnumerable<IHoverTip> AllTips =>
        [Tip, BandTip(Band.Sober), BandTip(Band.Tipsy), BandTip(Band.Drunk), BandTip(Band.Blackout)];
}
