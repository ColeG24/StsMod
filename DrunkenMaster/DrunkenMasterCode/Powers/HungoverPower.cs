using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>
/// Applied by Blackout at the end of a turn. For the next turn: 1 less Energy and draw 1 fewer card
/// per stack. Ticks down at the end of your turn; the tick on the turn it was applied is skipped
/// (SkipNextDurationTick) so it always lasts exactly the following turn.
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

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player && participants.Contains(Owner))
        {
            await PowerCmd.TickDownDuration(this);
        }
    }
}
