using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Rare;

/// <summary>
/// Rare Skill, 1 Energy. Gain 4 Block plus your current Intoxication. Blur 1: the Block is not removed at the start
/// of your next turn. Upgraded: 8 base Block (Blur stays 1). Uses the game's own BlurPower.
/// </summary>
public class Pickled() : DrunkenMasterCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public const string BlurKey = "Blur";

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(4),
        new CalculationExtraVar(1),
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier(
            (CardModel card, Creature? _) => IntoxicationResource.AmountOf(card.Owner)),
        new DynamicVar(BlurKey, 1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Block),
        IntoxicationResource.Tip,
        HoverTipFactory.FromPower<BlurPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var block = DynamicVars.CalculatedBlock;
        decimal amount = block.Calculate(Owner.Creature);
        if (amount > 0)
        {
            await CreatureCmd.GainBlock(Owner.Creature, amount, block.Props, cardPlay);
        }
        await PowerCmd.Apply<BlurPower>(choiceContext, Owner.Creature, DynamicVars[BlurKey].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.CalculationBase.UpgradeValueBy(4m);
}
