using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace DrunkenMaster.DrunkenMasterCode.Patches;

/// <summary>
/// Chaser's re-drink. Wraps the Task returned by PotionModel.OnUseWrapper: once the first use has fully resolved
/// (effects, history, every "whenever you drink" hook), spend a Chaser stack, put the potion back in a slot and run
/// the wrapper again with the same choice context and target. The second use is therefore a real use inside the SAME
/// game action on every machine (co-op safe), with a real choice context (Dregs' choose screen works), and every
/// drink power fires again. Recursion ends when the stacks run out.
///
/// If the re-drink cannot happen (target died, no slot), the Brew is given its seal check here, because
/// <see cref="BrewSealPatch"/> holds off while a Chaser stack is pending so the pot does not take the slot first.
/// </summary>
public static class ChaserPatch
{
    private static async Task DrinkAgain(Task original, PotionModel potion, PlayerChoiceContext choiceContext, Creature? target)
    {
        await original;
        var owner = potion.Owner;
        var creature = owner?.Creature;
        if (owner == null || creature == null || creature.CombatState == null) return;
        if (creature.IsDead || CombatManager.Instance.IsOverOrEnding) return;
        var chaser = creature.GetPower<ChaserPower>();
        if (chaser == null || chaser.Amount <= 0) return;

        if (target != null && !target.IsAlive)
        {
            await PowerCmd.Remove(chaser);
            await BrewSystem.TrySeal(owner);
            return;
        }

        // Spend the stack first so the re-drink's own wrapper postfix sees the reduced count.
        chaser.FlashSpent();
        if (chaser.Amount > 1) await PowerCmd.Decrement(chaser);
        else await PowerCmd.Remove(chaser);

        var result = await PotionCmd.TryToProcure(potion, owner);
        if (!result.success)
        {
            MainFile.Logger.Info($"Chaser: could not put {potion.Id.Entry} back to drink again ({result.failureReason}).");
            await BrewSystem.TrySeal(owner);
            return;
        }
        MainFile.Logger.Info($"Chaser: drinking {potion.Id.Entry} again.");
        await potion.OnUseWrapper(choiceContext, target);
    }

    [HarmonyPatch(typeof(PotionModel), nameof(PotionModel.OnUseWrapper))]
    public static class OnUseWrapper
    {
        public static void Postfix(PotionModel __instance, PlayerChoiceContext choiceContext, Creature? target, ref Task __result)
        {
            __result = DrinkAgain(__result, __instance, choiceContext, target);
        }
    }
}
