using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace DrunkenMaster.DrunkenMasterCode.Relics;

/// <summary>
/// Starter relic. At the start of combat, add a random Ingredient into your hand.
///
/// Also grants +2 potion slots on pickup (3 → 5). Spec §2 wants 5 slots as a character property,
/// but the game hard-codes Player.initialMaxPotionSlotCount = 3 with no CharacterModel override,
/// so this is the spec's own sanctioned fallback (open question 7). Replace with a Harmony patch
/// on Player construction if the relic-based approach proves awkward (e.g. Neow relic swap).
/// </summary>
public class TavernRag : DrunkenMasterRelic
{
    public const string IngredientsKey = "Ingredients";
    public const string PotionSlotsKey = "PotionSlots";

    public override RelicRarity Rarity => RelicRarity.Starter;
    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(IngredientsKey, 1),
        new DynamicVar(PotionSlotsKey, 2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(DrunkenMasterTips.Ingredient),
        HoverTipFactory.Static(DrunkenMasterTips.Brew)
    ];

    public override async Task AfterObtained()
    {
        await PlayerCmd.GainMaxPotionCount(DynamicVars[PotionSlotsKey].IntValue, Owner);
    }

    /// <summary>
    /// The slots are NOT taken back when the Rag leaves (Touch of Orobas, Neow swap): shrinking the belt would discard
    /// potions sitting in the top slots. Instead the count is parked here so the upgrade (Tavern Apron) can grant only
    /// the difference. In-memory only; the swap happens inside one command, so it never crosses a save.
    /// </summary>
    public static readonly Dictionary<Player, int> LingeringSlots = new();

    public override Task AfterRemoved()
    {
        LingeringSlots[Owner] = DynamicVars[PotionSlotsKey].IntValue;
        return Task.CompletedTask;
    }

    /// <summary>Touch of Orobas (via BaseLib's StarterUpgradePatches) swaps the Rag for this.</summary>
    public override RelicModel? GetUpgradeReplacement() => ModelDb.Relic<TavernApron>();

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || (player.PlayerCombatState?.TurnNumber ?? 1) > 1) return;
        Flash();
        await BrewSystem.AddRandomIngredientsToHand(Owner, DynamicVars[IngredientsKey].IntValue);
    }
}
