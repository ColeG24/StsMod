using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace DrunkenMaster.DrunkenMasterCode.Potions;

/// <summary>
/// What a Concoction turns into if it is still in a slot when combat ends (2026-09-12). Stateless, so it
/// survives save/reload with no per-instance data. Drinking it: choose 1 of 3 Ingredients to add to your hand.
/// </summary>
public class Dregs : DrunkenMasterPotion
{
    public const int Offer = 3;

    public override PotionRarity Rarity => PotionRarity.Token;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override bool CanBeGeneratedInCombat => false;
    public override TargetType TargetType => TargetType.AnyPlayer;

    public override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(DrunkenMasterTips.Ingredient),
        HoverTipFactory.Static(DrunkenMasterTips.Brew)
    ];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        await BrewSystem.ChooseIngredientsToHand(choiceContext, Owner, 1, Offer);
    }
}
