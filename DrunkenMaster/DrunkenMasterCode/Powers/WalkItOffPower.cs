using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>
/// Whenever the owner's Intoxication changes, gain Amount Block: gains (2026-09-17, user decision after testing) as
/// well as losses (turn-start decay, an Intoxication cost, the Blackout reset). Once per event, however many points
/// moved. Notified by <see cref="IntoxicationResource.GainAsync"/> and <see cref="IntoxicationResource.NotifyLost"/>.
/// Unpowered Block like a passive power's: Dexterity does not scale it.
/// </summary>
public class WalkItOffPower : DrunkenMasterPower, IntoxicationResource.ILossListener, IntoxicationResource.IGainListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip, HoverTipFactory.Static(StaticHoverTip.Block)];

    public Task OnIntoxicationLost(PlayerChoiceContext choiceContext, int amount) => OnChanged(amount);
    public Task OnIntoxicationGained(PlayerChoiceContext choiceContext, int amount) => OnChanged(amount);

    private async Task OnChanged(int amount)
    {
        if (amount <= 0 || Owner.IsDead) return;
        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
}
