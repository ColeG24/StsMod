using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Common;

/// <summary>
/// Common Skill (2026-09-15), 1 Energy. Gain 7 Block. Add a Rotgut into your hand. Upgraded: 10 Block and the Rotgut is
/// Upgraded. The plain 1-Energy Block Common the pool lost when Chug went Rare, and the Skill-side twin of Mash: Block now,
/// damage into the pot.
/// </summary>
public class Coaster() : DrunkenMasterCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<Rotgut>(IsUpgraded),
        HoverTipFactory.Static(DrunkenMasterTips.Brew)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        if (CombatState == null) return;
        var rotgut = BrewSystem.CreateIngredient(Owner, ModelDb.Card<Rotgut>(), upgraded: IsUpgraded);
        await CardPileCmd.AddGeneratedCardToCombat(rotgut, PileType.Hand, Owner);
    }
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}
