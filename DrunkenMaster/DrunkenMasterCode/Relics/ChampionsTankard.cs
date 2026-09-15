using DrunkenMaster.DrunkenMasterCode.Resources;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DrunkenMaster.DrunkenMasterCode.Relics;

/// <summary>
/// Rare. While Tipsy or Drunk you have 1 more Strength and 1 more Dexterity than the band grants (2026-09-15; was
/// "Tipsy = 3 / 3 instead of 2"). Read by <see cref="IntoxicationResource.BandPowersFor"/> whenever the band powers are
/// flushed; the resource remembers what it granted so a band change applies exactly the difference.
/// </summary>
public class ChampionsTankard : DrunkenMasterRelic, IntoxicationResource.IBandBonus
{
    public const string BonusKey = "Bonus";

    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(BonusKey, 1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        IntoxicationResource.BandTip(IntoxicationResource.Band.Tipsy),
        IntoxicationResource.BandTip(IntoxicationResource.Band.Drunk),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>()
    ];

    private int BonusFor(Player player, IntoxicationResource.Band band) =>
        player == Owner && band >= IntoxicationResource.Band.Tipsy ? DynamicVars[BonusKey].IntValue : 0;

    public int ExtraBandStrength(Player player, IntoxicationResource.Band band) => BonusFor(player, band);
    public int ExtraBandDexterity(Player player, IntoxicationResource.Band band) => BonusFor(player, band);
}
