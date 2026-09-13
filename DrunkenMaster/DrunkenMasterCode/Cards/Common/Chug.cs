using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Common;

/// <summary>Gain 5 Block. Gains 1 additional Block for each Intoxication. Upgraded: 8 base.</summary>
public class Chug() : DrunkenMasterCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(5),
        new CalculationExtraVar(1),
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier(
            (CardModel card, Creature? _) => IntoxicationResource.AmountOf(card.Owner))
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Block),
        IntoxicationResource.Tip
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var block = DynamicVars.CalculatedBlock;
        await CreatureCmd.GainBlock(Owner.Creature, block.Calculate(Owner.Creature), block.Props, cardPlay);
    }

    protected override void OnUpgrade() => DynamicVars.CalculationBase.UpgradeValueBy(3m);
}
