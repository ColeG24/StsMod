using DrunkenMaster.DrunkenMasterCode.Powers;
using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Uncommon;

/// <summary>Uncommon Skill, 1 Energy. Gain 4 Block. If you are Tipsy or above, take half damage from attacks until your next turn. Upgraded: 7 Block.</summary>
public class Numb() : DrunkenMasterCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(4, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<NumbPower>(), IntoxicationResource.Tip, IntoxicationResource.BandTip(IntoxicationResource.Band.Tipsy)];

    private bool TipsyOrAbove => IntoxicationResource.BandFor(IntoxicationResource.AmountOf(Owner)) >= IntoxicationResource.Band.Tipsy;
    protected override bool ShouldGlowGoldInternal => TipsyOrAbove;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        if (TipsyOrAbove)
            await PowerCmd.Apply<NumbPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}
