using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>
/// The first potion you drink each turn: gain Amount Energy and draw Amount cards.
///
/// Once per turn since 2026-09-14 (was every drink): with 0-cost Ingredients the Energy fed straight back into more
/// seals, and Chaser's re-drink double-dipped. The spent flag lives on the power instance (not a static) so co-op
/// stays in sync; it resets at the owner's turn start.
/// </summary>
public class BottomlessCupPower : DrunkenMasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private bool _usedThisTurn;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player) _usedThisTurn = false;
        return Task.CompletedTask;
    }

    public override async Task AfterPotionUsed(PotionModel potion, Creature? target)
    {
        if (potion.Owner != Owner.Player || _usedThisTurn) return;
        _usedThisTurn = true;
        Flash();
        await PlayerCmd.GainEnergy(Amount, Owner.Player);
        await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), Amount, Owner.Player);
    }
}
