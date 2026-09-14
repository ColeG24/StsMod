using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DrunkenMaster.DrunkenMasterCode.Potions;

/// <summary>
/// Rare character potion (2026-09-14). Add 3 random Ingredients into your hand: a full pot in one drink.
/// Self-targeted (not AnyPlayer like Dregs): Ingredients only mean something in the Drunken Master's hand.
/// </summary>
public class SpeedRail : DrunkenMasterPotion
{
    public const string IngredientsKey = "Ingredients";

    public override PotionRarity Rarity => PotionRarity.Rare;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(IngredientsKey, 3)];

    public override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(DrunkenMasterTips.Ingredient),
        HoverTipFactory.Static(DrunkenMasterTips.Brew)
    ];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        await BrewSystem.AddRandomIngredientsToHand(Owner, DynamicVars[IngredientsKey].IntValue);
    }
}
