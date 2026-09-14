using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>
/// The next potion you drink this turn is drunk again, once per stack. Wears off at end of turn.
///
/// The re-drink itself lives in <see cref="Patches.ChaserPatch"/>: after a potion's use wrapper finishes, the potion is
/// put back in a slot and the wrapper is run again, inside the same game action. Until 2026-09-14 this power enqueued a
/// second UsePotionAction from AfterPotionUsed, which desynced co-op: hooks run on every machine, so the host enqueued
/// the re-drink itself AND honoured the client's request for it (Block Potion drunk twice on the host, never on the
/// client). Rule: never enqueue actions from hooks; do the work inside the action that is already running.
/// </summary>
public class ChaserPower : DrunkenMasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>Called by the patch when a stack is spent (PowerModel.Flash is protected).</summary>
    public void FlashSpent() => Flash();

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player && participants.Contains(Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}
