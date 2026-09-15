using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DrunkenMaster.DrunkenMasterCode.Relics;

/// <summary>
/// Tavern Rag's Touch of Orobas upgrade (2026-09-14), the Black Blood to its Burning Blood: 2 Ingredients at the start
/// of combat (was 1) and 3 potion slots (was 2). Starter rarity like Black Blood so it never shows up in pools; it is
/// reached only through <see cref="TavernRag.GetUpgradeReplacement"/>.
///
/// Slot maths: the Rag's slots are never taken back (see <see cref="TavernRag.AfterRemoved"/>), so on pickup this
/// grants 3 minus whatever the Rag left behind. Spawned on its own (dev console) it grants the full 3.
/// </summary>
public class TavernApron : DrunkenMasterRelic
{
    public const string IngredientsKey = "Ingredients";
    public const string PotionSlotsKey = "PotionSlots";

    public override RelicRarity Rarity => RelicRarity.Starter;
    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(IngredientsKey, 2),
        new DynamicVar(PotionSlotsKey, 3)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(DrunkenMasterTips.Ingredient),
        HoverTipFactory.Static(DrunkenMasterTips.Brew)
    ];

    public override async Task AfterObtained()
    {
        int lingering = TavernRag.LingeringSlots.Remove(Owner, out var n) ? n : 0;
        int grant = DynamicVars[PotionSlotsKey].IntValue - lingering;
        if (grant > 0) await PlayerCmd.GainMaxPotionCount(grant, Owner);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || (player.PlayerCombatState?.TurnNumber ?? 1) > 1) return;
        Flash();
        await BrewSystem.AddRandomIngredientsToHand(Owner, DynamicVars[IngredientsKey].IntValue);
    }
}
