using BaseLib.Abstracts;
using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Uncommon;

/// <summary>0 Energy, X Intoxication (spends all). Gain 2 Block per Intoxication spent. Upgraded: 3. Needs at least 1.</summary>
public class SweatItOut : DrunkenMasterCard
{
    public const string BlockPerKey = "BlockPer";

    public SweatItOut() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
        CustomResources<IntoxicationResource>.SetXCost(this);
    }

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(BlockPerKey, 2)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip, HoverTipFactory.Static(StaticHoverTip.Block)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int spent = Math.Max(0, CustomResources<IntoxicationResource>.AmountSpent(cardPlay));
        decimal block = spent * DynamicVars[BlockPerKey].BaseValue;
        if (block > 0)
        {
            await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, cardPlay);
        }
    }

    protected override void OnUpgrade() => DynamicVars[BlockPerKey].UpgradeValueBy(1m);
}
