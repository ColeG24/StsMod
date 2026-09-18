using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>Start each turn at 12 Intoxication (Blackout). See <see cref="IntoxicationResource.ITurnStartFloor"/>.</summary>
public class BlackoutFormPower : DrunkenMasterPower, IntoxicationResource.ITurnStartFloor
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [IntoxicationResource.Tip, IntoxicationResource.BandTip(IntoxicationResource.Band.Blackout), HoverTipFactory.FromPower<HungoverPower>()];

    public int TurnStartMinimum(Player player) => player == Owner.Player ? IntoxicationResource.Max : 0;
    public void FlashTurnStart() => Flash();
}
