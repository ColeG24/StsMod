using DrunkenMaster.DrunkenMasterCode.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace DrunkenMaster.DrunkenMasterCode.Relics;

/// <summary>
/// Rare. Whenever you apply Confusion, apply 1 more. Rides the game's power-amount-given hook (the same one Snecko
/// Skull style relics use), so every source counts: cards, Jungle Juice in a Concoction, Bathtub Gin.
/// </summary>
public class AbsintheSpoon : DrunkenMasterRelic
{
    public const string ExtraKey = "Extra";

    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(ExtraKey, 1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ConfusionPower>()];

    public override decimal ModifyPowerAmountGivenAdditive(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
    {
        if (power is not ConfusionPower || giver != Owner.Creature || amount <= 0) return amount;
        return amount + DynamicVars[ExtraKey].BaseValue;
    }

    public override Task AfterModifyingPowerAmountGiven(PowerModel power)
    {
        if (power is ConfusionPower) Flash();
        return Task.CompletedTask;
    }
}
