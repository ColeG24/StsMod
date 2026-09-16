using BaseLib.Abstracts;
using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Common;

/// <summary>0 Energy + 2 Intoxication. Gain 7 Block. Upgraded: 10. Unplayable below 2 Intoxication. (2026-09-13: was 1 Energy for 9/12.)</summary>
public class ColdWater : DrunkenMasterCard
{
    public const int IntoxicationCost = 2;

    public ColdWater() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
        SetIntoxicationCost(IntoxicationCost);
    }

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(7, ValueProp.Move),
        new DynamicVar("IntoxicationCost", IntoxicationCost)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}
