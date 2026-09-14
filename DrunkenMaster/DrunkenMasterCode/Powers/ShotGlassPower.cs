using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Potions;
using DrunkenMaster.DrunkenMasterCode.Resources;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>
/// Your Brew holds 1 fewer Ingredient (the capacity change does not stack). Whenever you drink a Concoction, gain
/// Amount Intoxication (this part stacks).
///
/// 2026-09-14: the Intoxication line replaced +Strength/+Dexterity. A smaller pot is a payoff for every per-drink
/// power, so the card needed a cost that scales with the drinks it accelerates. Per drink, not per seal, so the
/// player still chooses when to get drunk. Base potions and Dregs do not count.
/// </summary>
public class ShotGlassPower : DrunkenMasterPower, BrewSystem.IPotCapacityModifier
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(DrunkenMasterTips.Brew),
        IntoxicationResource.Tip
    ];
    public int PotCapacityDelta(Player player) => player == Owner.Player ? -1 : 0;

    public override async Task AfterPotionUsed(PotionModel potion, Creature? target)
    {
        if (potion is not Concoction || potion.Owner != Owner.Player || Amount <= 0) return;
        Flash();
        await IntoxicationResource.GainAsync(new ThrowingPlayerChoiceContext(), Owner.Player, Amount);
    }
}
