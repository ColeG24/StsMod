using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>
/// Whenever the owner applies Confusion to an enemy, that enemy takes Amount damage. Same hook as the game's Vicious
/// (AfterPowerAmountChanged fires on a fresh application and on every stack added), so Karaoke Night, Jungle Juice in a
/// Concoction and Bathtub Gin all count. Unpowered damage: Strength and the target's Vulnerable stay out of it, and it is
/// not an attack, so Confusion never redirects it back.
/// </summary>
public class TheSpinsPower : DrunkenMasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ConfusionPower>()];

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (amount <= 0 || applier != Owner || power is not ConfusionPower) return;
        var target = power.Owner;
        if (target == null || target.Side == Owner.Side || !target.IsAlive) return;
        Flash();
        await CreatureCmd.Damage(choiceContext, target, Amount, ValueProp.Unpowered, Owner, null, null);
    }
}
