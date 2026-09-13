using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Powers;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Ancient;

/// <summary>
/// Ancient Power, 2 cost, Innate. Your Ingredients are Upgraded. Upgraded: costs 1.
/// This is the Drunken Master's Dusty Tome card: Darv's Ancient event picks a random Ancient-rarity card
/// from the character's pool and adds it to the deck already upgraded, so with the Tome this is an
/// Innate power costing 1 that you can play on turn 1.
/// </summary>
public class TopShelf() : DrunkenMasterCard(2, CardType.Power, CardRarity.Ancient, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<TopShelfPower>(),
        HoverTipFactory.Static(DrunkenMasterTips.Ingredient)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        NPowerUpVfx.CreateNormal(Owner.Creature);
        await PowerCmd.Apply<TopShelfPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
        await TopShelfPower.UpgradeExistingIngredients(Owner);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
