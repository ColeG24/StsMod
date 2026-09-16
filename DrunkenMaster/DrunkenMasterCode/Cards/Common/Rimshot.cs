using DrunkenMaster.DrunkenMasterCode.Resources;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Common;

/// <summary>
/// Common Attack (2026-09-15), 0 Energy + 3 Intoxication. Deal 8 damage. Apply 1 Weak and 1 Vulnerable. Upgraded: 12.
/// Merges Salt the Rim and Water It Down (0 + 1 Intoxication, 3 damage, one status each), which were twins and both
/// below the Regent yardstick: Falling Star is 0 Energy + 2 Stars for 8 (12), 1 Weak, 1 Vulnerable, and a Star is
/// worth about 1.5 Intoxication. Poised comes with the Intoxication cost.
/// </summary>
public class Rimshot : DrunkenMasterCard
{
    public const int IntoxicationCost = 3;
    public Rimshot() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        SetIntoxicationCost(IntoxicationCost);
    }
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8, ValueProp.Move),
        new PowerVar<WeakPower>(1),
        new PowerVar<VulnerablePower>(1),
        new DynamicVar("IntoxicationCost", IntoxicationCost)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        IntoxicationResource.Tip,
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
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
            await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, DynamicVars[nameof(VulnerablePower)].BaseValue, Owner.Creature, this);
        }
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}
