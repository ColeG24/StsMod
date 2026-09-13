using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Rare;

/// <summary>
/// Placeholder Rare so reward rolls always have a card for every rarity.
/// Skill, 1 cost, Exhaust. Gain 3 Intoxication. Draw 2 cards. Upgraded: draw 3.
/// </summary>
public class LastCall() : DrunkenMasterCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Intoxication", 3),
        new CardsVar(2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await IntoxicationResource.GainAsync(choiceContext, Owner, DynamicVars["Intoxication"].IntValue);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}
