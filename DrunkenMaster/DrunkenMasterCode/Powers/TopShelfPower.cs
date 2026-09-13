using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>
/// Your Ingredients are Upgraded. BrewSystem.CreateIngredient checks for this power when it makes
/// Ingredients; the generated-card hook below catches the few cards that create a specific Ingredient
/// directly (Hard Liquor, Slip a Mickey).
/// </summary>
public class TopShelfPower : DrunkenMasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(DrunkenMasterTips.Ingredient)];

    public override Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (card is IngredientCard && creator == Owner.Player && card.IsUpgradable)
        {
            CardCmd.Upgrade(card);
        }
        return Task.CompletedTask;
    }

    /// <summary>Upgrade every Ingredient the player already has in hand, in the draw/discard piles, and in the pot.</summary>
    public static Task UpgradeExistingIngredients(Player owner)
    {
        var state = owner.PlayerCombatState;
        if (state == null) return Task.CompletedTask;
        var inPiles = state.AllCards.OfType<IngredientCard>().Where(c => c.IsUpgradable).ToList();
        if (inPiles.Count > 0) CardCmd.Upgrade(inPiles, CardPreviewStyle.HorizontalLayout);
        bool potChanged = false;
        foreach (var brewed in BrewSystem.GetBrew(owner).Where(c => c.IsUpgradable))
        {
            brewed.UpgradeInternal();
            brewed.FinalizeUpgradeInternal();
            potChanged = true;
        }
        if (potChanged) BrewSystem.NotifyChanged(owner);
        return Task.CompletedTask;
    }
}
