using DrunkenMaster.DrunkenMasterCode.Powers;
using DrunkenMaster.DrunkenMasterCode.Resources;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Uncommon;

/// <summary>
/// Uncommon Attack (2026-09-15; was Common), 1 Energy + 1 Intoxication. Deal 10 damage. Apply 1 Weak and 1 Confusion.
/// Upgraded: 13, 2 Weak, 2 Confusion. The Confusion line is new with the move; its Common stat slot went to Sober Strike.
/// </summary>
public class BottleSmash : DrunkenMasterCard
{
    public const int IntoxicationCost = 1;
    public BottleSmash() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        SetIntoxicationCost(IntoxicationCost);
    }
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10, ValueProp.Move),
        new PowerVar<WeakPower>(1),
        new PowerVar<ConfusionPower>(1),
        new DynamicVar("IntoxicationCost", IntoxicationCost)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        IntoxicationResource.Tip,
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<ConfusionPower>()
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .Execute(choiceContext);
        if (cardPlay.Target.IsAlive)
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, DynamicVars[nameof(WeakPower)].BaseValue, Owner.Creature, this);
            await PowerCmd.Apply<ConfusionPower>(choiceContext, cardPlay.Target, DynamicVars[nameof(ConfusionPower)].BaseValue, Owner.Creature, this);
        }
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars[nameof(WeakPower)].UpgradeValueBy(1m);
        DynamicVars[nameof(ConfusionPower)].UpgradeValueBy(1m);
    }
}
