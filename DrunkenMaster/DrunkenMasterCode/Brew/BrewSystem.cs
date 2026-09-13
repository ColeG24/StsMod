using BaseLib.Utils;
using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Potions;
using DrunkenMaster.DrunkenMasterCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace DrunkenMaster.DrunkenMasterCode.Brew;

/// <summary>
/// Spec §4. The Brew is a 3-slot pot that lives for one combat. Playing an Ingredient adds it here;
/// at 3 it seals into a Concoction potion. If no potion slot is free the pot holds at 3 and refuses
/// further ingredients until a slot opens.
///
/// State is keyed on PlayerCombatState so it is discarded automatically when combat ends (rule 5).
/// The pot is rendered by <see cref="Ui.NBrewDisplay"/>, which listens to <see cref="Changed"/>.
/// TODO: persistence spike (spec §8) before Concoctions are allowed to outlive combat.
/// </summary>
public static class BrewSystem
{
    public const int BaseCapacity = 3;
    public const int MinCapacity = 1;
    public const int MaxCapacity = 5;

    /// <summary>Powers that change the pot size (Stockpot +1 per stack, Shot Glass -1).</summary>
    public interface IPotCapacityModifier
    {
        int PotCapacityDelta(Player player);
    }

    /// <summary>Pot size for this player right now: 3 plus every capacity power, clamped to 1..5.</summary>
    public static int CapacityFor(Player player)
    {
        int cap = BaseCapacity;
        var creature = player.Creature;
        if (creature != null)
            foreach (var mod in creature.Powers.OfType<IPotCapacityModifier>()) cap += mod.PotCapacityDelta(player);
        return Math.Clamp(cap, MinCapacity, MaxCapacity);
    }

    /// <summary>Call after a capacity power is applied: redraws the pot and seals it if it is now full.</summary>
    public static async Task RecheckSeal(Player owner)
    {
        Changed?.Invoke(owner);
        await TrySeal(owner);
    }

    /// <summary>Powers that react when a full pot seals into a Concoction (Moonshiner).</summary>
    public interface IBrewSealedListener
    {
        Task OnBrewSealed(Player owner, Concoction potion);
    }

    /// <summary>Raised whenever a player's pot contents change.</summary>
    public static event Action<Player>? Changed;

    /// <summary>Redraw the pot after its Ingredients were changed in place (Top Shelf upgrading them).</summary>
    public static void NotifyChanged(Player owner) => Changed?.Invoke(owner);

    private static readonly NotNullSpireField<PlayerCombatState, List<IngredientCard>> Brews = new(() => new List<IngredientCard>());

    /// <summary>Canonical ingredient models eligible for random generation (spec §5 starting pool).</summary>
    public static IReadOnlyList<IngredientCard> IngredientPool =>
    [
        ModelDb.Card<Rotgut>(),
        ModelDb.Card<Muddle>(),
        ModelDb.Card<Bitters>(),
        ModelDb.Card<HairOfTheDog>(),
        ModelDb.Card<GrainSpirit>(),
        ModelDb.Card<Ethanol>(),
        ModelDb.Card<Wormwood>(),
        ModelDb.Card<Everclear>(),
        ModelDb.Card<Seltzer>(),
        ModelDb.Card<JungleJuice>()
    ];

    public static IReadOnlyList<IngredientCard> GetBrew(Player player)
    {
        var state = player.PlayerCombatState;
        return state == null ? [] : Brews[state];
    }

    /// <summary>Called by IngredientCard.OnPlay.</summary>
    public static async Task AddIngredient(PlayerChoiceContext choiceContext, IngredientCard played)
    {
        var owner = played.Owner;
        var state = owner.PlayerCombatState;
        var combatState = owner.Creature.CombatState;
        if (state == null || combatState == null) return;

        var brew = Brews[state];
        if (brew.Count >= CapacityFor(owner))
        {
            // Spec rule 4: pot is full and blocked on a potion slot. Refuse the ingredient.
            MainFile.Logger.Info("Brew is full and no potion slot is free; ingredient refused.");
            await TrySeal(owner);
            return;
        }

        // Keep a fresh, owned copy of the ingredient (the played card is about to be exhausted).
        // Owned copies render correctly as card nodes in the pot display. The copy keeps the upgrade.
        var canonical = ModelDb.GetById<CardModel>(played.Id);
        var copy = CreateIngredient(owner, canonical, played.IsUpgraded);
        await AddToPot(owner, copy);
    }

    /// <summary>Put an owned Ingredient copy straight into the pot (no card play) and seal if it is now full.</summary>
    private static async Task AddToPot(Player owner, IngredientCard copy)
    {
        var state = owner.PlayerCombatState;
        if (state == null) return;
        var brew = Brews[state];
        brew.Add(copy);
        MainFile.Logger.Info($"Brew: added {copy.Id.Entry} ({brew.Count}/{CapacityFor(owner)})");
        Changed?.Invoke(owner);
        if (brew.Count >= CapacityFor(owner))
        {
            await TrySeal(owner);
        }
    }

    /// <summary>True while the player has Top Shelf: every Ingredient they make is Upgraded.</summary>
    public static bool UpgradesIngredients(Player owner) =>
        owner.Creature?.Powers.Any(p => p is TopShelfPower) ?? false;

    /// <summary>
    /// Every Ingredient the mod creates goes through here so Top Shelf (and callers that ask for it,
    /// like Restock+) can hand out Upgraded ones.
    /// </summary>
    public static IngredientCard CreateIngredient(Player owner, CardModel canonical, bool upgraded = false)
    {
        var card = (IngredientCard)owner.Creature.CombatState!.CreateCard(canonical, owner);
        if ((upgraded || UpgradesIngredients(owner)) && card.IsUpgradable)
        {
            card.UpgradeInternal();
            card.FinalizeUpgradeInternal();
        }
        return card;
    }

    private static List<CardModel> RandomDistinctIngredients(Player owner, int count, bool upgraded = false)
    {
        var rng = owner.RunState.Rng.CombatCardGeneration;
        var shuffled = IngredientPool.ToList();
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = rng.NextInt(i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }
        return shuffled.Take(Math.Min(count, shuffled.Count)).Select(c => (CardModel)CreateIngredient(owner, c, upgraded)).ToList();
    }

    /// <summary>Seal the full pot into a Concoction if a potion slot is free. Leaves the pot intact otherwise.</summary>
    public static async Task TrySeal(Player owner)
    {
        var state = owner.PlayerCombatState;
        if (state == null) return;
        var brew = Brews[state];
        if (brew.Count < CapacityFor(owner)) return;

        if (!owner.HasOpenPotionSlots)
        {
            MainFile.Logger.Info("Brew is ready but every potion slot is full; holding.");
            return;
        }

        var potion = ModelDb.Potion<Concoction>().ToMutable();
        Concoction.Ingredients[potion] = new List<IngredientCard>(brew);

        var result = await PotionCmd.TryToProcure(potion, owner);
        if (result.success)
        {
            MainFile.Logger.Info($"Brew sealed into Concoction: {string.Join(", ", brew.Select(i => i.Id.Entry))}");
            brew.Clear();
            Changed?.Invoke(owner);
            foreach (var listener in owner.Creature.Powers.OfType<IBrewSealedListener>().ToList())
            {
                await listener.OnBrewSealed(owner, (Concoction)potion);
            }
        }
        else
        {
            MainFile.Logger.Info($"Brew could not be sealed: {result.failureReason}");
        }
    }

    /// <summary>
    /// Open Bar: top the pot up with random Ingredients and seal it. Does nothing if the pot is already
    /// full and waiting on a potion slot.
    /// </summary>
    public static async Task FillWithRandomIngredients(Player owner)
    {
        var state = owner.PlayerCombatState;
        var combatState = owner.Creature.CombatState;
        if (state == null || combatState == null) return;
        var brew = Brews[state];
        var rng = owner.RunState.Rng.CombatCardGeneration;
        var pool = IngredientPool;
        int cap = CapacityFor(owner);
        while (brew.Count < cap)
        {
            var pick = pool[rng.NextInt(pool.Count)];
            brew.Add(CreateIngredient(owner, pick));
        }
        MainFile.Logger.Info($"Brew: filled by Open Bar ({string.Join(", ", brew.Select(i => i.Id.Entry))})");
        Changed?.Invoke(owner);
        await TrySeal(owner);
    }

    /// <summary>
    /// Free Pour: offer <paramref name="offer"/> distinct random Ingredients and let the player pick
    /// <paramref name="picks"/> of them (one screen per pick, no skipping). Picked cards go to hand.
    /// </summary>
    public static async Task ChooseIngredientsToHand(PlayerChoiceContext choiceContext, Player owner, int picks, int offer = 3)
    {
        var combatState = owner.Creature.CombatState;
        if (combatState == null || picks <= 0) return;
        var options = RandomDistinctIngredients(owner, offer);
        for (int i = 0; i < picks && options.Count > 0; i++)
        {
            var chosen = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, owner, canSkip: false);
            if (chosen == null) break;
            options.Remove(chosen);
            await CardPileCmd.AddGeneratedCardToCombat(chosen, PileType.Hand, owner);
        }
    }

    /// <summary>
    /// Generate <paramref name="count"/> random Ingredient tokens into the player's hand.
    /// <paramref name="upgraded"/> forces Upgraded ones (Restock+); Top Shelf upgrades them regardless.
    /// </summary>
    public static async Task AddRandomIngredientsToHand(Player owner, int count, bool upgraded = false)
    {
        var combatState = owner.Creature.CombatState;
        if (combatState == null || count <= 0) return;

        var rng = owner.RunState.Rng.CombatCardGeneration;
        var pool = IngredientPool;
        var cards = new List<CardModel>(count);
        for (int i = 0; i < count; i++)
        {
            var pick = pool[rng.NextInt(pool.Count)];
            cards.Add(CreateIngredient(owner, pick, upgraded));
        }

        await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, owner);
    }
}
