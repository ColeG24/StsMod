using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Potions;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>Whenever your Brew seals into a Concoction, gain Amount Energy.</summary>
public class MoonshinerPower : DrunkenMasterPower, BrewSystem.IBrewSealedListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(DrunkenMasterTips.Brew)];

    public async Task OnBrewSealed(Player owner, Concoction potion)
    {
        if (owner != Owner.Player) return;
        Flash();
        await PlayerCmd.GainEnergy(Amount, owner);
    }
}
