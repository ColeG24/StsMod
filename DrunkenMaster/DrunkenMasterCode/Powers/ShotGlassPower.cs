using BaseLib.Hooks;
using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Resources;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>
/// Your Brew holds 1 fewer Ingredient (the capacity change does not stack). Your Ingredients cost Amount more
/// Intoxication to play (this part stacks, so a second copy is a 1-pot at 2 per Ingredient).
///
/// 2026-09-19: the Intoxication-per-drink line was replaced by an Intoxication cost per Ingredient. A smaller pot
/// is a permanent +50% on every per-drink and per-seal payoff, and a rider that grants Intoxication was a second
/// payoff, not a price (the card was picked in 5 of the 6 runs it was offered in). Charging Intoxication per
/// Ingredient makes each extra potion cost something at the moment it is generated, and it makes the Brew deck
/// import Intoxication generation (Knock One Back, Bar Tab) rather than Intoxication payoffs. The cost rides
/// BaseLib's in-combat resource-cost hook, so it applies to every Ingredient the owner holds while the power is up
/// and vanishes with it; every Ingredient carries a canonical Intoxication cost of 0 so the cost object exists.
/// Cards that put Ingredients straight into the pot (Cellar Raid) bypass it on purpose: the text says "to play".
/// </summary>
public class ShotGlassPower : DrunkenMasterPower, BrewSystem.IPotCapacityModifier, IModifyResourceCostInCombat<IntoxicationResource>
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(DrunkenMasterTips.Brew),
        HoverTipFactory.Static(DrunkenMasterTips.Ingredient),
        IntoxicationResource.Tip
    ];
    public int PotCapacityDelta(Player player) => player == Owner.Player ? -1 : 0;

    public decimal ModifyResourceCostInCombat(CardModel card, IntoxicationResource resource, decimal originalCost) =>
        card is IngredientCard && card.Owner == Owner.Player ? originalCost + Amount : originalCost;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        RefreshIngredientCosts(Owner.Player);
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        RefreshIngredientCosts(oldOwner.Player);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Redraw the Intoxication badge on every Ingredient in hand. The hook is read live, but a card node only
    /// repaints its costs on its own refresh; this makes the new price show the moment the power lands or stacks
    /// (ShotGlass.OnPlay calls it too, because AfterApplied does not run for a stack).
    /// </summary>
    public static void RefreshIngredientCosts(Player? player)
    {
        var hand = player?.PlayerCombatState?.Hand;
        if (hand == null) return;
        foreach (var card in hand.Cards.OfType<IngredientCard>().ToList())
        {
            card.InvokeEnergyCostChanged();
            NCard.FindOnTable(card)?.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);
        }
    }
}
