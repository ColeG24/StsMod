using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>
/// The next potion you drink this turn is drunk again, once per stack. Wears off at end of turn.
///
/// Each extra drink is a real use: the potion is put back in a slot and a normal use action is queued,
/// so it gets its own animation, a real player-choice context (Dregs' choose screen works), and every
/// "whenever you drink a potion" power fires again. One stack is spent per re-drink.
/// </summary>
public class ChaserPower : DrunkenMasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPotionUsed(PotionModel potion, Creature? target)
    {
        var owner = Owner.Player;
        if (owner == null || potion.Owner != owner || Amount <= 0) return;
        if (CombatManager.Instance.IsOverOrEnding) return;
        if (target != null && !target.IsAlive) { await PowerCmd.Remove(this); return; }

        // Spend the stack first so the re-drink's own AfterPotionUsed doesn't chain forever.
        if (Amount > 1) await PowerCmd.Decrement(this);
        else await PowerCmd.Remove(this);

        var result = await PotionCmd.TryToProcure(potion, owner);
        if (!result.success)
        {
            MainFile.Logger.Info($"Chaser: could not put {potion.Id.Entry} back to drink again ({result.failureReason}).");
            return;
        }
        MainFile.Logger.Info($"Chaser: drinking {potion.Id.Entry} again.");
        potion.EnqueueManualUse(target);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player && participants.Contains(Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}
