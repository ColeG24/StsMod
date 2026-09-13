using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>At the start of your turn, apply Amount Confusion to ALL enemies.</summary>
public class KaraokeNightPower : DrunkenMasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ConfusionPower>()];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        var combatState = Owner.CombatState;
        if (combatState == null) return;
        Flash();
        foreach (var enemy in combatState.HittableEnemies.ToList())
        {
            if (!enemy.IsAlive) continue;
            await PowerCmd.Apply<ConfusionPower>(choiceContext, enemy, Amount, Owner, null);
        }
    }
}
