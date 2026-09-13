using DrunkenMaster.DrunkenMasterCode.Powers;
using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Uncommon;

/// <summary>Uncommon Skill, 1 Energy. Gain 4 Intoxication if you are Sober, otherwise 2. Upgraded: 6 / 3.</summary>
public class Pregame() : DrunkenMasterCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public const string SoberKey = "SoberIntoxication";
    public const string OtherwiseKey = "Intoxication";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(SoberKey, 4),
        new DynamicVar(OtherwiseKey, 2)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip];

    private bool IsSober => IntoxicationResource.BandFor(IntoxicationResource.AmountOf(Owner)) == IntoxicationResource.Band.Sober;
    protected override bool ShouldGlowGoldInternal => IsSober;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var key = IsSober ? SoberKey : OtherwiseKey;
        await IntoxicationResource.GainAsync(choiceContext, Owner, DynamicVars[key].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[SoberKey].UpgradeValueBy(2m);
        DynamicVars[OtherwiseKey].UpgradeValueBy(1m);
    }
}
