using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>Whenever you drink a potion, gain Amount Energy and draw Amount cards.</summary>
public class BottomlessCupPower : DrunkenMasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPotionUsed(PotionModel potion, Creature? target)
    {
        if (potion.Owner != Owner.Player) return;
        Flash();
        await PlayerCmd.GainEnergy(Amount, Owner.Player);
        await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), Amount, Owner.Player);
    }
}
