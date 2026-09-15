using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Uncommon;

/// <summary>
/// Uncommon Skill, 2 Energy (2026-09-14; was Common, 1 Energy, 5/8 Block). Apply 1 Weak. Gain 11 Block. Add a Wormwood
/// into your hand. Upgraded: 2 Weak, 14 Block, and the Wormwood is Upgraded.
/// </summary>
public class SlipAMickey() : DrunkenMasterCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<WeakPower>(1),
        new BlockVar(11, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromCard<Wormwood>(IsUpgraded),
        HoverTipFactory.Static(DrunkenMasterTips.Brew)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, DynamicVars[nameof(WeakPower)].BaseValue, Owner.Creature, this);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        if (CombatState == null) return;
        var wormwood = BrewSystem.CreateIngredient(Owner, ModelDb.Card<Wormwood>(), upgraded: IsUpgraded);
        await CardPileCmd.AddGeneratedCardToCombat(wormwood, PileType.Hand, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(WeakPower)].UpgradeValueBy(1m);
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
