using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>Whenever you Blackout, play Amount additional cards from the top of your draw pile (on top of the usual 3).</summary>
public class BenderPower : DrunkenMasterPower, DrunkenMasterBands.IBlackoutCardBonus, DrunkenMasterBands.IBlackoutListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        IntoxicationResource.Tip,
        IntoxicationResource.BandTip(IntoxicationResource.Band.Blackout),
        HoverTipFactory.FromPower<HungoverPower>()
    ];

    public int ExtraBlackoutCards() => Amount;

    public Task OnBlackout(PlayerChoiceContext choiceContext)
    {
        Flash();
        return Task.CompletedTask;
    }
}
