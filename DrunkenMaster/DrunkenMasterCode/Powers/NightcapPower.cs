using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>
/// Next turn, gain Amount Intoxication (applied with the turn-start decay as one change, like Bar Tab),
/// then the power removes itself. Playing another Nightcap stacks the amount.
/// </summary>
public class NightcapPower : DrunkenMasterPower, IntoxicationResource.IPerTurnSource
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip];

    public int IntoxicationPerTurn(Player player) => player == Owner.Player ? Amount : 0;
    public void FlashPerTurn() => Flash();

    public async Task AfterTurnStartApplied(PlayerChoiceContext choiceContext)
    {
        await PowerCmd.Remove(this);
    }
}
