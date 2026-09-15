using DrunkenMaster.DrunkenMasterCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Rare;

/// <summary>
/// Rare Attack, 3 Energy. Apply 8 Confusion, then deal 8 damage plus the target's current Confusion (the stacks just
/// applied included, so 16 minimum). Upgraded: 12 Confusion and 12 base damage.
/// </summary>
public class Vertigo() : DrunkenMasterCard(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ConfusionPower>(8),
        new DamageVar(8, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ConfusionPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var target = cardPlay.Target;
        await PowerCmd.Apply<ConfusionPower>(choiceContext, target, DynamicVars[nameof(ConfusionPower)].BaseValue, Owner.Creature, this);
        if (!target.IsAlive) return;   // The Spins can kill off the application
        decimal confusion = target.Powers.OfType<ConfusionPower>().FirstOrDefault()?.Amount ?? 0;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + confusion)
            .FromCard(this, cardPlay)
            .Targeting(target)
            .WithHitFx("vfx/vfx_heavy_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(ConfusionPower)].UpgradeValueBy(4m);
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
