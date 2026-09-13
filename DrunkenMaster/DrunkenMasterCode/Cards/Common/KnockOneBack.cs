using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Common;

/// <summary>
/// 0 Energy. Gain 2 Intoxication. Draw 1 card. Upgraded: 3 Intoxication.
/// The cheap fuel card for the Intoxication-cost Commons (Bottle Smash, Hurl, Cold Water, Sway, Pick-Me-Up).
/// </summary>
public class KnockOneBack() : DrunkenMasterCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public const string IntoxicationKey = "Intoxication";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(IntoxicationKey, 2),
        new CardsVar(1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await IntoxicationResource.GainAsync(choiceContext, Owner, DynamicVars[IntoxicationKey].IntValue);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade() => DynamicVars[IntoxicationKey].UpgradeValueBy(1m);
}
