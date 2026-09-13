using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>
/// Read-only status icon under the character showing the current band (Sober / Tipsy / Drunk /
/// Blackout) with the Intoxication amount as its number. Its title and description switch with
/// the band. Applied once at the start of combat by <see cref="DrunkenMasterBands"/> and kept in
/// sync by listening to the resource. The stack count is always 1 so the game never removes it.
/// </summary>
public class DrunkennessPower : DrunkenMasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string CustomPackedIconPath => MainFile.ResPath + "/images/ui/intoxication.png";
    public override string CustomBigIconPath => MainFile.ResPath + "/images/ui/intoxication_big.png";

    private IntoxicationResource? Resource => Owner?.Player == null ? null : IntoxicationResource.Get(Owner.Player);

    private int Intoxication => Resource?.Amount ?? 0;

    private string BandKey => IntoxicationResource.BandFor(Intoxication) switch
    {
        IntoxicationResource.Band.Tipsy => "tipsy",
        IntoxicationResource.Band.Drunk => "drunk",
        IntoxicationResource.Band.Blackout => "blackout",
        _ => "sober"
    };

    public override int DisplayAmount => Intoxication;

    public override LocString Title => new("powers", $"{Id.Entry}.{BandKey}.title");
    public override LocString Description => new("powers", $"{Id.Entry}.{BandKey}.description");
    protected override string SmartDescriptionLocKey => $"{Id.Entry}.{BandKey}.smartDescription";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip];

    private IntoxicationResource? _listening;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        Listen();
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        if (_listening != null) _listening.AmountChanged -= OnAmountChanged;
        _listening = null;
        return Task.CompletedTask;
    }

    private void Listen()
    {
        var res = Resource;
        if (res == null || _listening == res) return;
        if (_listening != null) _listening.AmountChanged -= OnAmountChanged;
        _listening = res;
        res.AmountChanged += OnAmountChanged;
    }

    private void OnAmountChanged(int _, int __) => InvokeDisplayAmountChanged();
}
