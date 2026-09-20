using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>
/// If you end your turn Drunk (or in a Blackout), your Block is not removed at the start of your next turn.
///
/// 2026-09-19 (built as DeadDrunkPower, renamed the same day when it took over the Iron Liver slot from the Vigor-per-drink version). The class had no defensive Rare Power; every base character has one to three, and Walk It Off already
/// owns "Intoxication becomes Block", so this is the rule-change template (Barricade) with a band gate instead of a
/// second Block source. Armed at BeforeSideTurnEnd if the owner is Drunk or above, or by a Blackout (which fires in the
/// auto-post-play phase, BEFORE BeforeSideTurnEnd, and resets Intoxication to 0, so ending the turn at 12 would
/// otherwise never count). Read by ShouldClearBlock at the owner's next turn start, and re-evaluated at the following
/// turn end, so the check never depends on where the Block clear sits relative to the turn-start hooks. Plain fields
/// like BottomlessCupPower's once-per-turn flag.
/// </summary>
public class IronLiverPower : DrunkenMasterPower, DrunkenMasterBands.IBlackoutListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        IntoxicationResource.Tip,
        IntoxicationResource.BandTip(IntoxicationResource.Band.Drunk),
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];

    private bool _keepBlock;
    private bool _blackedOutThisTurn;

    public Task OnBlackout(PlayerChoiceContext choiceContext)
    {
        _blackedOutThisTurn = true;
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player) _blackedOutThisTurn = false;
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        var owner = Owner.Player;
        if (owner == null || !participants.Contains(Owner)) return Task.CompletedTask;
        bool drunk = IntoxicationResource.Get(owner)?.CurrentBand >= IntoxicationResource.Band.Drunk;
        _keepBlock = drunk || _blackedOutThisTurn;
        if (_keepBlock) Flash();
        return Task.CompletedTask;
    }

    public override bool ShouldClearBlock(Creature creature) => creature != Owner || !_keepBlock;
}
