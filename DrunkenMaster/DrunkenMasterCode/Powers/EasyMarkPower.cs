using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>
/// The owner's Attacks deal Amount% more damage to enemies with Confusion. Owner-only: the Power sits on the player, not
/// on the enemy, so allies' attacks in co-op get nothing (2026-09-16 user ruling). A pure multiplicative modifier keyed on
/// the target, so the Confusion damage patch treats it like Vulnerable: it scales the part that lands on the enemy and
/// never the part redirected back at it.
/// </summary>
public class EasyMarkPower : DrunkenMasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ConfusionPower>()];

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (dealer != Owner || target == null || !props.IsPoweredAttack()) return 1m;
        if (target.Side == Owner.Side || !target.HasPower<ConfusionPower>()) return 1m;
        return 1m + Amount / 100m;
    }
}
