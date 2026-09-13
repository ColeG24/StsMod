using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>
/// While Drunk, the random cost a card gets can never be higher than its current cost. The clamp lives in
/// <see cref="IntoxicationResource.RandomizeCost"/>, which checks for this power.
/// </summary>
public class TolerancePower : DrunkenMasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip];
}
