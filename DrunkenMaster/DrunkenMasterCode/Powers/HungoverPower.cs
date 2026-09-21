using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>
/// Applied by Blackout at the end of a turn. For the next turn: 1 less Energy and draw 1 fewer card.
/// Ticks down at the end of your turn; the tick on the turn it was applied is skipped
/// (SkipNextDurationTick) so it always lasts exactly the following turn. A second Blackout while already
/// Hungover calls <see cref="Refresh"/> instead of applying again, so it stays at 1 (2026-09-19; it used to stack).
/// </summary>
public class HungoverPower : DrunkenMasterPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        return player == Owner.Player ? Math.Max(0, count - Amount) : count;
    }

    public override async Task AfterEnergyReset(Player player)
    {
        if (player != Owner.Player) return;
        Flash();
        await PlayerCmd.LoseEnergy(Amount, player);
    }

    /// <summary>
    /// Another Blackout resolved while this is still up: keep it for one more turn without adding a stack. The end-of-turn
    /// tick that would have removed it tonight is skipped, exactly as a fresh application would be.
    /// </summary>
    public void Refresh()
    {
        SkipNextDurationTick = true;
        Flash();
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player && participants.Contains(Owner))
        {
            await PowerCmd.TickDownDuration(this);
        }
    }
}
