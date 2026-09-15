using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DrunkenMaster.DrunkenMasterCode.Relics;

/// <summary>Shop. Your Brew holds 1 more Ingredient (4 to seal a Concoction). Same hook as the Stockpot power.</summary>
public class CopperKettle : DrunkenMasterRelic, BrewSystem.IPotCapacityModifier
{
    public const string SlotsKey = "Slots";

    public override RelicRarity Rarity => RelicRarity.Shop;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(SlotsKey, 1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(DrunkenMasterTips.Brew),
        HoverTipFactory.Static(DrunkenMasterTips.Ingredient)
    ];

    public int PotCapacityDelta(Player player) => player == Owner ? DynamicVars[SlotsKey].IntValue : 0;
}
