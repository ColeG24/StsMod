using DrunkenMaster.DrunkenMasterCode.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Patches;

/// <summary>
/// Applies <see cref="ConfusionPower"/> to attack damage. Two patches:
///
/// 1. A postfix on Hook.ModifyDamage, the game's single damage-modification entry point (real hits, card previews
///    and enemy intents all go through it). It is the only place that sees the base damage as well as the result after
///    every additive/multiplicative/cap hook, and it is PURE: it only rewrites the returned amount.
///
/// 2. A prefix on CreatureCmd.Damage, the one place real damage is dealt. It records, per target, how much of the hit
///    is redirected to the attacker; <see cref="ConfusionPower.AfterDamageGiven"/> consumes that and deals it.
///
/// The record must NOT live in the ModifyDamage postfix: enemy intents call ModifyDamage for the LOCAL player only,
/// so in multiplayer each machine would write machine-specific entries interleaved with the real attack. That desynced
/// a 2-player run on 2026-09-13 (Flail Knight HP diverged by 12 during its Confused 2-hit attack).
///
/// The redirected amount is measured against the attack "as the dealer throws it": the same hooks re-run with no
/// target, which is how the game itself previews untargeted damage, so target-side modifiers (Vulnerable, Intangible)
/// drop out. redirected = min(Confusion, outgoing). The final damage is scaled by (outgoing - redirected) / outgoing,
/// which is exactly "subtract before the target's multipliers" since the multiplicative hooks commute.
/// </summary>
public static class ConfusionDamagePatch
{
    [ThreadStatic] private static bool _computingOutgoing;

    /// <summary>Whole-number damage the dealer would deal before the target's own modifiers. Deterministic.</summary>
    private static int OutgoingDamage(IRunState runState, ICombatState? combatState, Creature dealer, decimal damage,
        ValueProp props, CardModel? cardSource, CardPlay? cardPlay, ModifyDamageHookType hookType, decimal fallback)
    {
        decimal outgoing;
        _computingOutgoing = true;
        try
        {
            outgoing = Hook.ModifyDamage(runState, combatState, null, dealer, damage, props, cardSource, cardPlay,
                hookType, CardPreviewMode.None, out _);
        }
        catch (Exception e)
        {
            MainFile.Logger.Error($"Confusion: untargeted damage pass failed, falling back to the targeted amount. {e}");
            outgoing = fallback;
        }
        finally
        {
            _computingOutgoing = false;
        }
        return (int)Math.Max(0m, outgoing);
    }

    private static bool Applies(Creature? dealer, Creature? target, ValueProp props, out ConfusionPower confusion)
    {
        confusion = null!;
        if (dealer == null || target == null || dealer == target || dealer.Side == target.Side) return false;
        if (!props.IsPoweredAttack()) return false;
        var power = dealer.GetPower<ConfusionPower>();
        if (power == null || power.Amount <= 0) return false;
        confusion = power;
        return true;
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.ModifyDamage))]
    public static class ReduceDamage
    {
        public static void Postfix(IRunState runState, ICombatState? combatState, Creature? target, Creature? dealer,
            decimal damage, ValueProp props, CardModel? cardSource, CardPlay? cardPlay,
            ModifyDamageHookType modifyDamageHookType, ref decimal __result, ref IEnumerable<AbstractModel> modifiers)
        {
            if (_computingOutgoing) return;
            if (!modifyDamageHookType.HasFlag(ModifyDamageHookType.Multiplicative)) return;
            if (!Applies(dealer, target, props, out var confusion)) return;

            int outgoing = OutgoingDamage(runState, combatState, dealer!, damage, props, cardSource, cardPlay, modifyDamageHookType, __result);
            int redirected = Math.Min(confusion.Amount, outgoing);
            if (redirected <= 0) return;

            __result = __result * (outgoing - redirected) / outgoing;
            modifiers = modifiers.Append(confusion);
        }
    }

    [HarmonyPatch(typeof(CreatureCmd), nameof(CreatureCmd.Damage),
        typeof(PlayerChoiceContext), typeof(IEnumerable<Creature>), typeof(decimal), typeof(ValueProp), typeof(Creature), typeof(CardModel), typeof(CardPlay))]
    public static class RecordSelfDamage
    {
        public static void Prefix(ref IEnumerable<Creature>? targets, decimal amount, ValueProp props, Creature? dealer,
            CardModel? cardSource, CardPlay? cardPlay)
        {
            if (targets == null || dealer == null || dealer.IsDead) return;
            var list = targets.ToList();
            targets = list;   // the method enumerates it again; keep it a stable list
            if (list.Count == 0) return;
            var combatState = dealer.CombatState;
            var runState = IRunState.GetFrom(list.Append(dealer).OfType<Creature>());
            int? outgoing = null;
            foreach (var target in list)
            {
                if (target.IsDead || !Applies(dealer, target, props, out var confusion)) continue;
                outgoing ??= OutgoingDamage(runState, combatState, dealer, amount, props, cardSource, cardPlay, ModifyDamageHookType.All, amount);
                confusion.RecordSelfDamage(target, Math.Min(confusion.Amount, outgoing.Value));
            }
        }
    }
}
