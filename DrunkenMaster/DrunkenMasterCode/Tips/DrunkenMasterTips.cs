using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.HoverTips;

namespace DrunkenMaster.DrunkenMasterCode.Tips;

/// <summary>
/// Static hover tips for the mod's mechanics. Reference these as DrunkenMasterTips.Ingredient etc.
/// Localization lives in static_hover_tips.json under DRUNKENMASTER-INGREDIENT / DRUNKENMASTER-BREW.
/// </summary>
public static class DrunkenMasterTips
{
    [CustomEnum] public static StaticHoverTip Ingredient;
    [CustomEnum] public static StaticHoverTip Brew;
    // One tip per Intoxication band (2026-09-14). Cards show only the band they check.
    [CustomEnum] public static StaticHoverTip Sober;
    [CustomEnum] public static StaticHoverTip Tipsy;
    [CustomEnum] public static StaticHoverTip Drunk;
    [CustomEnum] public static StaticHoverTip Blackout;
}
