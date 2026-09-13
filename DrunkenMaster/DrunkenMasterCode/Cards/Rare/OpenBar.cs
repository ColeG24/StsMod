using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Rare;

/// <summary>1 Energy, Exhaust. Fill your Brew with random Ingredients (which seals a Concoction if a slot is free). Upgraded: costs 0.</summary>
public class OpenBar() : DrunkenMasterCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(DrunkenMasterTips.Brew),
        HoverTipFactory.Static(DrunkenMasterTips.Ingredient)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await BrewSystem.FillWithRandomIngredients(Owner);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
