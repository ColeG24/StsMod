using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>
/// Whenever the owner loses Intoxication (turn-start decay, an Intoxication cost, the Blackout reset), gain Amount
/// Block. Once per loss event, however many points were lost. Notified by <see cref="IntoxicationResource.NotifyLost"/>.
/// Unpowered Block like a passive power's: Dexterity does not scale it.
/// </summary>
public class WalkItOffPower : DrunkenMasterPower, IntoxicationResource.ILossListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip, HoverTipFactory.Static(StaticHoverTip.Block)];

    public async Task OnIntoxicationLost(PlayerChoiceContext choiceContext, int amount)
    {
        if (amount <= 0 || Owner.IsDead) return;
        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
}
