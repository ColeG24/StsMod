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
///   Tipsy+ : card attacks deal +2, card block gives +2.
///   Drunk+ : cards you draw get a random cost 0–3 until played (Confused / Snecko precedent).
///            Entering Drunk also re-rolls the hand (see IntoxicationResource.OnBandChanged).
///   Blackout (12): at the end of that turn, play the top 3 cards of your draw pile, Exhaust your
///                  hand, reset to 0, and apply Hungover for the next turn.
/// </summary>
public class DrunkenMasterBands() : CustomSingletonModel(HookType.Combat)
{
    private static bool IsDrunkenMaster(Player? player) => player?.Character is Character.DrunkenMaster;

    private static IntoxicationResource.Band BandOf(Player player) =>
        IntoxicationResource.BandFor(IntoxicationResource.AmountOf(player));

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        var player = dealer?.Player;
        if (cardSource == null || !IsDrunkenMaster(player) || !props.IsPoweredAttack()) return 0m;
        return BandOf(player!) >= IntoxicationResource.Band.Tipsy ? IntoxicationResource.TipsyDamageBonus : 0m;
    }

    public override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        var player = target.Player;
        if (cardSource == null || !IsDrunkenMaster(player)) return 0m;
        return BandOf(player!) >= IntoxicationResource.Band.Tipsy ? IntoxicationResource.TipsyBlockBonus : 0m;
    }

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
        var hungover = await PowerCmd.Apply<HungoverPower>(choiceContext, player.Creature, 1, player.Creature, null);
        if (hungover != null) hungover.SkipNextDurationTick = true;   // lasts the *next* turn, not the one ending now
    }
}
