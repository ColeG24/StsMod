using BaseLib.Abstracts;
using DrunkenMaster.DrunkenMasterCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Resources;

/// <summary>
/// Passive combat hooks that give the Intoxication bands their teeth.
///   Tipsy+ : 2 Strength and 2 Dexterity, granted/removed by IntoxicationResource.FlushBandPowers
///            (2026-09-14; was hidden +2 damage / +2 Block hooks here).
///   Drunk+ : cards you draw get a random cost 0–3 until played (Confused / Snecko precedent).
///            Entering Drunk also re-rolls the hand (see IntoxicationResource.OnBandChanged).
///   Blackout (12): at the end of that turn, play the top 3 cards of your draw pile, reset to 0, and apply
///                  Hungover for the next turn. The hand is discarded normally (2026-09-14; it used to be Exhausted).
/// </summary>
public class DrunkenMasterBands() : CustomSingletonModel(HookType.Combat)
{
    /// <summary>Powers on the player's creature that react to a Blackout resolving (Dutch Courage).</summary>
    public interface IBlackoutListener
    {
        Task OnBlackout(PlayerChoiceContext choiceContext);
    }

    private static bool IsDrunkenMaster(Player? player) => player?.Character is Character.DrunkenMaster;

    private static IntoxicationResource.Band BandOf(Player player) =>
        IntoxicationResource.BandFor(IntoxicationResource.AmountOf(player));

    /// <summary>Put the band status icon under every Drunken Master at the start of combat.</summary>
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        foreach (var creature in participants)
        {
            var player = creature.Player;
            if (!IsDrunkenMaster(player) || creature.HasPower<DrunkennessPower>()) continue;
            if ((player!.PlayerCombatState?.TurnNumber ?? 1) > 1) continue;
            await PowerCmd.Apply<DrunkennessPower>(new ThrowingPlayerChoiceContext(), creature, 1, creature, null, silent: true);
        }
    }

    /// <summary>
    /// Runs after the energy reset and before the hand draw. Decay and per-turn gains (Bar Tab) land
    /// here as a single net change so the band never flickers.
    /// </summary>
    public override async Task AfterEnergyReset(Player player)
    {
        if (!IsDrunkenMaster(player)) return;
        await IntoxicationResource.ApplyTurnStart(new ThrowingPlayerChoiceContext(), player);
    }

    public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        var player = card.Owner;
        if (!IsDrunkenMaster(player) || BandOf(player) < IntoxicationResource.Band.Drunk) return Task.CompletedTask;
        IntoxicationResource.RandomizeCost(player, card);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Blackout trigger. This is the engine's per-player "auto-play at end of turn" hook (Stampede uses it), and it
    /// arrives with a HookPlayerChoiceContext OWNED BY THIS PLAYER. That matters in co-op: a hook context can carry
    /// exactly one synchronized hook action, and every player choice must happen inside it. The previous home,
    /// BeforeSideTurnEnd, gives a singleton like this ONE context for the whole side, owned by the first player, and
    /// running two Blackouts (or one Blackout with two prompts) through it made the second choice skip
    /// synchronization ("Tried to interrupt action ... but the currently running action is ...") and desynced a
    /// 2-player game on 2026-09-14 (Free Pour+ resolved on the host only).
    /// </summary>
    public override async Task AfterAutoPostPlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
    {
        if (!IsDrunkenMaster(player) || player.Creature is not { IsDead: false }) return;
        var resource = IntoxicationResource.Get(player);
        if (resource == null || resource.Amount < IntoxicationResource.BlackoutAt) return;
        await Blackout(choiceContext, player, resource);
    }

    /// <summary>
    /// Blackout (2026-09-14 rewrite): play the top cards of the draw pile, then sober up and wake Hungover. The hand is
    /// no longer Exhausted; the normal end-of-turn discard takes it (user decision after the 2026-09-14 co-op soft-lock).
    ///
    /// Written defensively because it runs inside an end-of-turn hook on every machine:
    /// - Nothing here mutates the hand, so an auto-played card that opens a prompt (Distill, Leftovers, Free Pour+)
    ///   sees the same hand on host and client.
    /// - The sober-up and Hungover land in a finally block: if an auto-played card throws, the Intoxication still
    ///   resets, so the next turn cannot fire a second Blackout off the same 12.
    /// - Every step re-checks that combat is still running and the player is alive (a Blackout card can kill you).
    /// </summary>
    private static async Task Blackout(PlayerChoiceContext choiceContext, Player player, IntoxicationResource resource)
    {
        MainFile.Logger.Info("Blackout!");
        try
        {
            await CardPileCmd.AutoPlayFromDrawPile(choiceContext, player, IntoxicationResource.BlackoutCardsPlayed, CardPilePosition.Top, forceExhaust: false);
        }
        finally
        {
            if (!CombatManager.Instance.IsOverOrEnding && player.Creature is { IsDead: false })
            {
                resource.Amount = 0;
                await resource.FlushBandPowers(choiceContext);
            }
        }
        if (CombatManager.Instance.IsOverOrEnding || player.Creature is not { IsDead: false } creature) return;

        var hungover = await PowerCmd.Apply<HungoverPower>(choiceContext, creature, 1, creature, null);
        if (hungover != null) hungover.SkipNextDurationTick = true;   // lasts the *next* turn, not the one ending now

        foreach (var listener in creature.Powers.OfType<IBlackoutListener>().ToList())
        {
            if (CombatManager.Instance.IsOverOrEnding || creature.IsDead) return;
            await listener.OnBlackout(choiceContext);
        }
    }
}
