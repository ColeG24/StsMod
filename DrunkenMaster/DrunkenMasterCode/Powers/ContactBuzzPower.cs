using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>
/// Whenever the owner attacks an enemy, apply Amount Confusion to it. Monarch's Gaze shape: the after-damage-given hook,
/// so it is per hit and per target (a 3-hit card applies 3, an AoE applies to every enemy) and fires whether or not the
/// hit was blocked. Applied with the owner as applier and no card source, so Absinthe Spoon adds its +1 and The Spins
/// deals its damage per hit (intended; that is the two-Power combo). Confusion still expires at end of turn.
/// </summary>
public class ContactBuzzPower : DrunkenMasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ConfusionPower>()];

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != Owner || !props.IsPoweredAttack()) return;
        if (target == Owner || target.Side == Owner.Side || !target.IsAlive) return;
        Flash();
        await PowerCmd.Apply<ConfusionPower>(choiceContext, target, Amount, Owner, null);
    }
}
