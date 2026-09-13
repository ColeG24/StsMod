using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>Whenever you Blackout (end a turn at 12 Intoxication), gain Amount Strength. 2026-09-13: was "whenever your band goes up".</summary>
public class DutchCouragePower : DrunkenMasterPower, DrunkenMasterBands.IBlackoutListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        IntoxicationResource.Tip
    ];

    public async Task OnBlackout(PlayerChoiceContext choiceContext)
    {
        Flash();
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, Amount, Owner, null);
    }
}
