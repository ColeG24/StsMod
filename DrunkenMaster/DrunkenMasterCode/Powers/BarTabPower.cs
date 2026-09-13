using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>At the start of your turn, gain Amount Intoxication (netted against the 1-point decay, applied as one change).</summary>
public class BarTabPower : DrunkenMasterPower, IntoxicationResource.IPerTurnSource
{
    public int IntoxicationPerTurn(Player player) => player == Owner.Player ? Amount : 0;
    public void FlashPerTurn() => Flash();

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip];

    // The gain itself is applied by IntoxicationResource.ApplyTurnStart, netted against the decay.
}
