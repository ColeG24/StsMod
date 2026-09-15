using DrunkenMaster.DrunkenMasterCode.Powers;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Patches;

/// <summary>
/// Shows the damage a Confused enemy will deal to itself next turn on its health bar, the way Poison is shown: a
/// tinted segment at the end of the red bar, with the red bar shortened to the HP it will have left (2026-09-14).
///
/// The segment is a tinted duplicate of the HP foreground, inserted once per bar, placed after Poison's segment
/// (Poison ticks at the start of the enemy's turn, before it attacks). The amount is computed exactly like the real
/// hit will be: per attack intent, the dealer-side damage (Hook.ModifyDamage with no target, so Strength and Weak
/// count and the player's Vulnerable does not) capped at the Confusion stacks, times the hit count.
///
/// Multiplayer: this is display-only and reads shared, deterministic state (the enemy's rolled move, its powers, the
/// combat history), so every machine draws the same thing and nothing is written. A move that hits every player
/// redirects once per player per hit; that factor is learned from the move's previous use this combat (default 1). The bar redraws on every CombatStateChanged, which
/// covers Confusion changes, Strength changes and new move rolls. Block is ignored, like Poison: enemy Block is gone by
/// the time it attacks unless the move itself grants some first.
/// </summary>
[HarmonyPatch(typeof(NHealthBar), "RefreshForeground")]
public static class ConfusionHealthBarPatch
{
    private const string OverlayName = "ConfusionForeground";
    private static readonly Color OverlayColor = new("b48cff");

    /// <summary>
    /// Damage the creature's next move will redirect onto itself through its own Confusion. 0 if none.
    /// Every hit on every target redirects once, so an attack that hits both players in co-op counts twice per hit
    /// (the way Thorns on each player would). Intents do not say whether a move hits one player or all of them, so
    /// that factor is learned from this combat's history (<see cref="TargetsPerHit"/>) and defaults to 1.
    /// </summary>
    public static int SelfDamageNextTurn(Creature creature)
    {
        try
        {
            if (creature.IsDead || creature.Monster == null) return 0;
            var confusion = creature.GetPower<ConfusionPower>();
            if (confusion == null || confusion.Amount <= 0) return 0;
            var combatState = creature.CombatState;
            if (combatState == null) return 0;

            int total = 0, intentHits = 0;
            foreach (var intent in creature.Monster.NextMove.Intents.OfType<AttackIntent>())
            {
                if (intent.DamageCalc == null) continue;
                decimal outgoing = Hook.ModifyDamage(combatState.RunState, combatState, null, creature, intent.DamageCalc(),
                    ValueProp.Move, null, null, ModifyDamageHookType.All, CardPreviewMode.None, out _);
                int perHit = Math.Min(confusion.Amount, (int)Math.Max(0m, outgoing));
                int hits = Math.Max(1, intent.Repeats);
                total += perHit * hits;
                intentHits += hits;
            }
            if (total <= 0) return 0;
            return total * TargetsPerHit(creature, creature.Monster.NextMove.Id, intentHits, combatState);
        }
        catch (Exception e)
        {
            MainFile.Logger.Error($"Confusion health bar preview failed: {e}");
            return 0;
        }
    }

    /// <summary>
    /// How many creatures each hit of <paramref name="moveId"/> landed on the last time this monster used it: the
    /// number of damage results its attacks produced divided by the hits its intents advertised. 1 until the move has
    /// been seen once. The history is built from the shared action stream, so every machine learns the same number.
    /// </summary>
    private static int TargetsPerHit(Creature creature, string moveId, int intentHits, ICombatState combatState)
    {
        if (intentHits <= 0) return 1;
        int alivePlayers = Math.Max(1, combatState.PlayerCreatures.Count(c => c.IsAlive));
        if (alivePlayers == 1) return 1;
        int learned = 1, pendingResults = 0;
        foreach (var entry in CombatManager.Instance.History.Entries)
        {
            if (entry is CreatureAttackedEntry attacked && attacked.Actor == creature)
            {
                pendingResults += attacked.DamageResults.Count;
            }
            else if (entry is MonsterPerformedMoveEntry performed && performed.Monster == creature.Monster)
            {
                if (performed.Move.Id == moveId && pendingResults > 0)
                    learned = Math.Clamp((int)Math.Round(pendingResults / (double)intentHits), 1, alivePlayers);
                pendingResults = 0;
            }
        }
        return learned;
    }

    private static Control GetOrCreateOverlay(Control hpForeground)
    {
        var parent = hpForeground.GetParent();
        var existing = parent.GetNodeOrNull<Control>(OverlayName);
        if (existing != null) return existing;
        var overlay = (Control)hpForeground.Duplicate();
        overlay.Name = OverlayName;
        overlay.UniqueNameInOwner = false;
        overlay.SelfModulate = OverlayColor;
        overlay.Visible = false;
        parent.AddChild(overlay);
        parent.MoveChild(overlay, hpForeground.GetIndex() + 1);
        return overlay;
    }

    private static float FgWidth(Creature creature, int amount, float maxFgWidth)
    {
        if (creature.MaxHp <= 0) return 0f;
        float val = (float)amount / creature.MaxHp * maxFgWidth;
        return Math.Max(val, creature.CurrentHp > 0 ? 12f : 0f);
    }

    public static void Postfix(Creature ____creature, Control ____hpForeground, Control ____poisonForeground,
        Control ____hpForegroundContainer, float ____expectedMaxFgWidth)
    {
        var creature = ____creature;
        var hpForeground = ____hpForeground;
        if (creature == null || hpForeground == null) return;
        var overlay = GetOrCreateOverlay(hpForeground);

        if (creature.CurrentHp <= 0 || creature.HpDisplay.IsInfinite())
        {
            overlay.Visible = false;
            return;
        }
        int confusion = SelfDamageNextTurn(creature);
        if (confusion <= 0)
        {
            overlay.Visible = false;
            return;
        }

        int poison = ____poisonForeground.Visible ? creature.GetPower<PoisonPower>()?.CalculateTotalDamageNextTurn() ?? 0 : 0;
        int hpAfterPoison = creature.CurrentHp - poison;
        if (hpAfterPoison <= 0)
        {
            overlay.Visible = false;   // Poison already kills it; nothing left to show
            return;
        }

        float maxFg = ____expectedMaxFgWidth > 0f ? ____expectedMaxFgWidth : ____hpForegroundContainer.Size.X;
        float rightEdge = FgWidth(creature, hpAfterPoison, maxFg) - maxFg;
        overlay.Visible = true;
        if (confusion >= hpAfterPoison)
        {
            overlay.OffsetLeft = 0f;
            overlay.OffsetRight = rightEdge;
            hpForeground.Visible = false;
        }
        else
        {
            float fgWidth = FgWidth(creature, hpAfterPoison - confusion, maxFg);
            hpForeground.OffsetRight = fgWidth - maxFg;
            hpForeground.Visible = true;
            int patchMargin = overlay is NinePatchRect nine ? nine.PatchMarginLeft : 0;
            overlay.OffsetLeft = Math.Max(0f, fgWidth - patchMargin);
            overlay.OffsetRight = rightEdge;
        }
    }
}
