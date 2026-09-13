using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>Whenever your Intoxication band goes up, gain Amount Strength.</summary>
public class DutchCouragePower : DrunkenMasterPower, IntoxicationResource.IBandRaisedListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        IntoxicationResource.Tip
    ];

    public async Task OnBandRaised(PlayerChoiceContext choiceContext, IntoxicationResource.Band from, IntoxicationResource.Band to)
    {
        Flash();
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, Amount, Owner, null);
    }
}
