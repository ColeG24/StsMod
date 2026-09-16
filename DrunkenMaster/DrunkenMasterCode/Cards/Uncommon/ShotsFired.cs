using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Uncommon;

/// <summary>
/// Uncommon Attack, 2 Energy (2026-09-16). Deal 5 damage to a random enemy once, plus once more for each potion you have
/// drunk this combat (Sword Boomerang targeting). Upgraded: 7. The hit count is a live CalculatedVar (base 1 + 1 per drink)
/// so the card shows the real number. Drinks are counted from combat history's PotionUsedEntry, filtered to this player:
/// Dregs count, a Chaser double-trigger counts once. The potion archetype's first Attack; it double-scales with Strength
/// from Tipsy and Beer Muscles because every hit gets it.
/// </summary>
public class ShotsFired() : DrunkenMasterCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.RandomEnemy)
{
    public const string HitsKey = "Hits";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(5, ValueProp.Move),
        new CalculationBaseVar(1),
        new CalculationExtraVar(1),
        new CalculatedVar(HitsKey).WithMultiplier(CountPotionsDrunk)
    ];

    private static decimal CountPotionsDrunk(CardModel card, Creature? _)
    {
        var creature = card.Owner.Creature;
        return CombatManager.Instance.History.Entries.OfType<PotionUsedEntry>().Count(e => e.Actor == creature);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null) return;
        int hits = Math.Max(1, (int)((CalculatedVar)DynamicVars[HitsKey]).Calculate(null));
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(hits)
            .FromCard(this, cardPlay)
            .TargetingRandomOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}
