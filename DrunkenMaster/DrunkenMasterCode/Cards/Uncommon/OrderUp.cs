using DrunkenMaster.DrunkenMasterCode.Tips;
using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Powers;
using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Uncommon;

/// <summary>Uncommon Skill, 1 Energy. Gain 7 Block. Next turn, add a Grain Spirit into your hand. Upgraded: 10 Block (2026-09-14: was a random Ingredient, and the upgrade added a second).</summary>
public class OrderUp() : DrunkenMasterCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7, ValueProp.Move), new PowerVar<OrderUpPower>(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<OrderUpPower>(),
        HoverTipFactory.FromCard<GrainSpirit>(),
        HoverTipFactory.Static(DrunkenMasterTips.Brew)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<OrderUpPower>(choiceContext, Owner.Creature, DynamicVars[nameof(OrderUpPower)].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}
