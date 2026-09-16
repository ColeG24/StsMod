using DrunkenMaster.DrunkenMasterCode.Powers;
using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Common;

/// <summary>
/// 1 Energy. Gain 2 Intoxication. Next turn, gain 3 Intoxication. Upgraded: 4. (2026-09-15 night; was 1 now. From the combat
/// start of 3 the base card is Tipsy now and 7 next turn; the upgrade lands on 8, Drunk next turn.)
/// </summary>
public class Nightcap() : DrunkenMasterCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public const string NowKey = "Intoxication";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(NowKey, 2),
        new PowerVar<NightcapPower>(3)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        IntoxicationResource.Tip,
        HoverTipFactory.FromPower<NightcapPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await IntoxicationResource.GainAsync(choiceContext, Owner, DynamicVars[NowKey].IntValue);
        await PowerCmd.Apply<NightcapPower>(choiceContext, Owner.Creature, DynamicVars[nameof(NightcapPower)].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars[nameof(NightcapPower)].UpgradeValueBy(1m);
}
