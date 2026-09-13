using DrunkenMaster.DrunkenMasterCode.Powers;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Patches;

/// <summary>
/// Applies <see cref="ConfusionPower"/> to attack damage. Hook.ModifyDamage is the game's single damage-modification
/// entry point (real hits, card previews and enemy intents all go through it), and it is the only place that sees the
/// base damage as well as the result after every additive/multiplicative/cap hook has run.
///
/// The value the hit is reduced by is measured against the attack "as the dealer throws it": the same hooks re-run
/// with no target, which is how the game itself previews untargeted damage, so target-side modifiers (Vulnerable,
/// Intangible) drop out. The redirected amount is min(Confusion, that outgoing damage). The final damage is scaled by
/// (outgoing - redirected) / outgoing, which is exactly "subtract before the target's multipliers" since the
/// multiplicative hooks commute.
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyDamage))]
public static class ConfusionDamagePatch
{
    [ThreadStatic] private static bool _computingOutgoing;

    public static void Postfix(IRunState runState, ICombatState? combatState, Creature? target, Creature? dealer,
        decimal damage, ValueProp props, CardModel? cardSource, CardPlay? cardPlay,
        ModifyDamageHookType modifyDamageHookType, ref decimal __result, ref IEnumerable<AbstractModel> modifiers)
    {
        if (_computingOutgoing) return;
        if (dealer == null || target == null || dealer == target || dealer.Side == target.Side) return;
        if (!props.IsPoweredAttack() || !modifyDamageHookType.HasFlag(ModifyDamageHookType.Multiplicative)) return;
        var confusion = dealer.GetPower<ConfusionPower>();
        if (confusion == null || confusion.Amount <= 0) return;

        decimal outgoing;
        _computingOutgoing = true;
        try
        {
            outgoing = Hook.ModifyDamage(runState, combatState, null, dealer, damage, props, cardSource, cardPlay,
                modifyDamageHookType, CardPreviewMode.None, out _);
        }
        catch (Exception e)
        {
            MainFile.Logger.Error($"Confusion: untargeted damage pass failed, falling back to the targeted amount. {e}");
            outgoing = __result;
        }
        finally
        {
            _computingOutgoing = false;
        }

        int outgoingInt = (int)Math.Max(0m, outgoing);
        int redirected = Math.Min(confusion.Amount, outgoingInt);
        confusion.RecordSelfDamage(target, redirected);
        if (redirected <= 0) return;

        __result = __result * (outgoingInt - redirected) / outgoingInt;
        modifiers = modifiers.Append(confusion);
    }
}
