using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>
/// Confusion. Each of the owner's attack hits deals Amount less damage, and the damage taken off the hit is dealt to
/// the owner instead (never more than the hit would have done: 2x3 with 5 Confusion is 0x3 to the target and 2x3 to
/// the owner). The reduction comes after the owner's own modifiers (Strength, Weak) and before the target's
/// (Vulnerable), so a Vulnerable target does not make the self-hit bigger. Wears off at the end of the owner's turn,
/// however many stacks it has.
///
/// The reduction itself is done by <see cref="Patches.ConfusionDamagePatch"/>, a postfix on Hook.ModifyDamage: it is
/// the only place that sees both the base damage and the fully modified damage, and it also covers the intent preview.
/// The patch records the redirected amount per target here; the self-hit is dealt in AfterDamageGiven.
/// </summary>
public class ConfusionPower : DrunkenMasterPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private readonly Dictionary<Creature, int> _pendingSelfDamage = new();

    /// <summary>Called by the damage patch just before a hit on <paramref name="target"/> lands.</summary>
    public void RecordSelfDamage(Creature target, int amount) => _pendingSelfDamage[target] = amount;

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != Owner || target == Owner || !props.IsPoweredAttack()) return;
        if (!_pendingSelfDamage.Remove(target, out var selfDamage) || selfDamage <= 0) return;
        if (Owner.IsDead) return;
        Flash();
        // Unpowered like Thorns: the owner's Strength / the player's Vulnerable must not touch it. No dealer so the
        // owner plays its hurt animation (the game skips it when dealer == receiver).
        await CreatureCmd.Damage(choiceContext, Owner, selfDamage, ValueProp.Unpowered, null, null, null);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == Owner.Side && participants.Contains(Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}
