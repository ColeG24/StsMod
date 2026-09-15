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
/// power icon. Shown as a dial above the draw pile. Decays 1 at the start of your turn. Resets each combat.
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

    public const int DecayPerTurn = 1;

    /// <summary>
    /// The amount after a turn start from <paramref name="current"/>: decay and every per-turn source
    /// are summed and applied as ONE change, so a Bar Tab at the edge of a band never dips out of it
    /// and back in (which would re-roll the hand and re-trigger band-up powers every turn).
    /// </summary>
    private static int NextFrom(int current, Player player)
    {
        int next = current - DecayPerTurn;
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
        bool wasTipsy = from >= Band.Tipsy, isTipsy = to >= Band.Tipsy;
        if (wasTipsy != isTipsy) _pendingTipsy += isTipsy ? 1 : -1;
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
        // Tolerance (Rare Power): the roll can only match or lower the card's current cost.
        if (player.Creature?.HasPower<Powers.TolerancePower>() == true) cost = Math.Min(cost, card.EnergyCost.GetResolved());
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

    /// <summary>Tipsy and above grant real Strength and Dexterity (2026-09-14; was hidden +2 damage / +2 Block hooks).</summary>
    public const int TipsyStrength = 2;
    public const int TipsyDexterity = 2;
    public const int BlackoutCardsPlayed = 3;

    /// <summary>Relics that raise the Tipsy buff (Champion's Tankard: +1 / +1).</summary>
    public interface ITipsyBonus
    {
        int ExtraTipsyStrength(Player player);
        int ExtraTipsyDexterity(Player player);
    }

    public static int TipsyStrengthFor(Player player) =>
        TipsyStrength + player.Relics.OfType<ITipsyBonus>().Sum(r => r.ExtraTipsyStrength(player));
    public static int TipsyDexterityFor(Player player) =>
        TipsyDexterity + player.Relics.OfType<ITipsyBonus>().Sum(r => r.ExtraTipsyDexterity(player));

    /// <summary>What crossing the Tipsy line actually granted, so leaving it takes back exactly that much.</summary>
    private int _grantedStrength, _grantedDexterity;

    /// <summary>
    /// Band transitions are noticed in the sync Amount setter, but granting a power is a command. Crossing the
    /// Tipsy line queues +1 / -1 here and every async write path (GainAsync, ApplyTurnStart, Spend, the Blackout
    /// reset) calls <see cref="FlushBandPowers"/> straight after, so the powers land in the action stream on
    /// every machine. Removal is a plain -2 like Flex: Strength stolen in between can leave you below where you
    /// started, which is the base game's own precedent.
    /// </summary>
    private int _pendingTipsy;

    public async Task FlushBandPowers(PlayerChoiceContext choiceContext)
    {
        var creature = Owner?.Creature;
        var owner = Owner;
        while (_pendingTipsy != 0 && creature != null && owner != null && !creature.IsDead)
        {
            int sign = Math.Sign(_pendingTipsy);
            _pendingTipsy -= sign;
            int str, dex;
            if (sign > 0)
            {
                str = TipsyStrengthFor(owner); dex = TipsyDexterityFor(owner);
                _grantedStrength += str; _grantedDexterity += dex;
            }
            else
            {
                str = -_grantedStrength; dex = -_grantedDexterity;
                _grantedStrength = 0; _grantedDexterity = 0;
            }
            if (str != 0) await PowerCmd.Apply<StrengthPower>(choiceContext, creature, str, creature, null);
            if (dex != 0) await PowerCmd.Apply<DexterityPower>(choiceContext, creature, dex, creature, null);
        }
        _pendingTipsy = 0;
    }

    /// <summary>Cards that cost Intoxication spend through here; dropping below Tipsy must take the powers away.</summary>
    public override async Task<bool> Spend<T>(ICombatState combatState, AbstractModel? spender, int amount, bool optional)
    {
        bool ok = await base.Spend<T>(combatState, spender, amount, optional);
        await FlushBandPowers(new ThrowingPlayerChoiceContext());
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
        Gain(player, amount);
        var to = res.CurrentBand;
        await res.FlushBandPowers(choiceContext);
        if (to <= from || player.Creature == null) return;
        foreach (var listener in player.Creature.Powers.OfType<IBandRaisedListener>().ToList())
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
