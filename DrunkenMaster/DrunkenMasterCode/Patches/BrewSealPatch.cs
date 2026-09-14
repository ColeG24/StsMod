using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace DrunkenMaster.DrunkenMasterCode.Patches;

/// <summary>
/// Seals a full Brew the moment a potion slot opens (2026-09-14). Before this the pot only re-checked on the next
/// Ingredient play, so a full pot sat unsealed after you drank, and that next Ingredient was lost.
///
/// Both patches wrap the returned Task of the game's hook fan-out (Hook.AfterPotionUsed / Hook.AfterPotionDiscarded)
/// so the seal runs AFTER every power and relic listener. While a Chaser stack is pending the seal is skipped: the
/// re-drink (<see cref="ChaserPatch"/>, which runs after the whole use wrapper) needs the freed slot, and its own use
/// triggers this patch again once the stacks are spent. Entropic Brew fills its own old slot inside its use, so the pot
/// stays full and waits for the next opening.
///
/// Skipped once combat is over or ending: Concoction.AfterCombatEnd discards potions into Dregs, and a Concoction
/// sealed at that point would outlive the fight with per-instance state (the spec §8 save problem).
/// </summary>
public static class BrewSealPatch
{
    private static async Task SealAfter(Task original, PotionModel potion)
    {
        await original;
        var owner = potion.Owner;
        if (owner == null) return;
        if (owner.Creature?.CombatState == null) return;
        if (CombatManager.Instance.IsOverOrEnding) return;
        var chaser = owner.Creature.GetPower<ChaserPower>();
        if (chaser != null && chaser.Amount > 0) return;   // ChaserPatch seals after the re-drink (or when it cannot happen)
        await BrewSystem.TrySeal(owner);
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterPotionUsed))]
    public static class AfterUsed
    {
        public static void Postfix(PotionModel potion, Creature? target, ref Task __result)
        {
            __result = SealAfter(__result, potion);
        }
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterPotionDiscarded))]
    public static class AfterDiscarded
    {
        public static void Postfix(PotionModel potion, ref Task __result)
        {
            __result = SealAfter(__result, potion);
        }
    }
}
