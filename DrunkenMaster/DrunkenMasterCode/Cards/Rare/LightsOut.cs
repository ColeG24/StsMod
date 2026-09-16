using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Rare;

/// <summary>
/// 0 Energy, Exhaust. Gain 9 Intoxication. Upgraded: 12. (2026-09-15 late; was 12 + draw 1 (2). From the combat-start 3
/// the base card is exactly Blackout; a spent-down dial needs the upgrade or a top-up.) This is the spec's "the player may
/// trigger Blackout" as a card.
/// </summary>
public class LightsOut() : DrunkenMasterCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Intoxication", 9)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await IntoxicationResource.GainAsync(choiceContext, Owner, DynamicVars["Intoxication"].IntValue);
    }

    protected override void OnUpgrade() => DynamicVars["Intoxication"].UpgradeValueBy(3m);
}
