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
///   Blackout (12): at the end of that turn, play the top 3 cards of your draw pile, Exhaust your
///                  hand, reset to 0, and apply Hungover for the next turn.
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

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;
        foreach (var creature in participants.ToList())
        {
            var player = creature.Player;
            if (!IsDrunkenMaster(player) || creature.IsDead) continue;
            var resource = IntoxicationResource.Get(player!);
            if (resource == null || resource.Amount < IntoxicationResource.BlackoutAt) continue;

            await Blackout(choiceContext, player!, resource);
        }
    }

    private static async Task Blackout(PlayerChoiceContext choiceContext, Player player, IntoxicationResource resource)
    {
        MainFile.Logger.Info("Blackout!");
        await CardPileCmd.AutoPlayFromDrawPile(choiceContext, player, IntoxicationResource.BlackoutCardsPlayed, CardPilePosition.Top, forceExhaust: false);
        if (CombatManager.Instance.IsOverOrEnding) return;

        // Whatever is left in hand is lost to the night: Exhaust it instead of letting the normal
        // end-of-turn discard take it. This hook runs before the game's hand flush.
        var hand = player.PlayerCombatState?.Hand;
        if (hand != null)
        {
            foreach (var card in hand.Cards.ToList())
            {
                if (CombatManager.Instance.IsOverOrEnding) return;
                await CardCmd.Exhaust(choiceContext, card);
            }
        }

        resource.Amount = 0;
        await resource.FlushBandPowers(choiceContext);
        var hungover = await PowerCmd.Apply<HungoverPower>(choiceContext, player.Creature, 1, player.Creature, null);
        if (hungover != null) hungover.SkipNextDurationTick = true;   // lasts the *next* turn, not the one ending now

        foreach (var listener in player.Creature.Powers.OfType<IBlackoutListener>().ToList())
        {
            if (CombatManager.Instance.IsOverOrEnding) return;
            await listener.OnBlackout(choiceContext);
        }
    }
}
