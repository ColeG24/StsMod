using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>
/// The first time the owner is Drunk (or past it, at Blackout) each turn, gain Amount Energy. Two ways in: the turn
/// starts Drunk (checked in AfterPlayerTurnStart, which runs after the turn-start decay has landed, so waking at 8 and
/// decaying to 6 does not count), or a gain crosses into Drunk mid turn (<see cref="IntoxicationResource.GainAsync"/>,
/// which also covers a Bar Tab pushing you over at turn start; that runs after the energy reset, so the Energy sticks).
///
/// Once per turn: the turn it last paid out on is remembered, so sobering up and drinking back into Drunk pays
/// nothing. The turn number lives on the power instance (not a static) so co-op stays in sync, and it advances before
/// the turn-start hooks run, so it needs no reset hook.
/// </summary>
public class TolerancePower : DrunkenMasterPower, IntoxicationResource.IBandRaisedListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip, IntoxicationResource.BandTip(IntoxicationResource.Band.Drunk)];

    private int _paidOutOnTurn = -1;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player) await TryPayOut();
    }

    public async Task OnBandRaised(PlayerChoiceContext choiceContext, IntoxicationResource.Band from, IntoxicationResource.Band to)
    {
        if (from < IntoxicationResource.Band.Drunk && to >= IntoxicationResource.Band.Drunk) await TryPayOut();
    }

    /// <summary>Pay out if the owner is Drunk on their own turn and this turn has not paid yet. Also called by the card on play.</summary>
    public async Task TryPayOut()
    {
        var player = Owner.Player;
        if (player?.PlayerCombatState == null || Owner.IsDead) return;
        // Intoxication gained on the enemy's turn must not burn the payout; the next turn start picks it up.
        if (Owner.CombatState?.CurrentSide != CombatSide.Player) return;
        if (IntoxicationResource.BandFor(IntoxicationResource.AmountOf(player)) < IntoxicationResource.Band.Drunk) return;
        int turn = player.PlayerCombatState.TurnNumber;
        if (_paidOutOnTurn == turn) return;
        _paidOutOnTurn = turn;
        Flash();
        await PlayerCmd.GainEnergy(Amount, player);
    }
}
