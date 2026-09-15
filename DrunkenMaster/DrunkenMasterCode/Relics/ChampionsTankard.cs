using DrunkenMaster.DrunkenMasterCode.Resources;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DrunkenMaster.DrunkenMasterCode.Relics;

/// <summary>
/// Rare. Tipsy grants 3 Strength and 3 Dexterity instead of 2. Read by <see cref="IntoxicationResource.TipsyStrengthFor"/>
/// when the Tipsy line is crossed; the resource remembers what it granted so sobering up takes back the same amount.
/// </summary>
public class ChampionsTankard : DrunkenMasterRelic, IntoxicationResource.ITipsyBonus
{
    public const string BonusKey = "Bonus";
    public const string TotalKey = "Total";

    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(BonusKey, 1),
        new DynamicVar(TotalKey, IntoxicationResource.TipsyStrength + 1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        IntoxicationResource.BandTip(IntoxicationResource.Band.Tipsy),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>()
    ];

    public int ExtraTipsyStrength(Player player) => player == Owner ? DynamicVars[BonusKey].IntValue : 0;
    public int ExtraTipsyDexterity(Player player) => player == Owner ? DynamicVars[BonusKey].IntValue : 0;
}
