using DrunkenMaster.DrunkenMasterCode.Resources;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Rare;

/// <summary>
/// Rare Skill (2026-09-15 evening; was Common), 1 Energy. Gain 5 Block. Twice if Tipsy, 4 times if Drunk (or Blackout,
/// until it resolves). Upgraded: 7. Poised so the Drunk re-roll cannot tax the card you got Drunk for.
/// History: 5 (8) +1 per Intoxication; 7 (10) doubled at Tipsy+ (2026-09-15 am); 6 (8) doubled (2026-09-15 pm).
/// </summary>
public class Chug() : DrunkenMasterCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public const int TipsyTimes = 2;
    public const int DrunkTimes = 4;

    public override bool GainsBlock => true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [DrunkenMasterKeywords.Poised];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5m, ValueProp.Move),
        new DynamicVar("TipsyTimes", TipsyTimes),
        new DynamicVar("DrunkTimes", DrunkTimes)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Block),
        IntoxicationResource.Tip,
        IntoxicationResource.BandTip(IntoxicationResource.Band.Tipsy),
        IntoxicationResource.BandTip(IntoxicationResource.Band.Drunk)
    ];

    private IntoxicationResource.Band Band => IntoxicationResource.BandFor(IntoxicationResource.AmountOf(Owner));

    private int Times => Band switch
    {
        IntoxicationResource.Band.Sober => 1,
        IntoxicationResource.Band.Tipsy => TipsyTimes,
        _ => DrunkTimes
    };

    protected override bool ShouldGlowGoldInternal => Band >= IntoxicationResource.Band.Tipsy;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int times = Times;   // read once: the Block gains themselves never move the dial
        for (int i = 0; i < times; i++)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(2m);
}
