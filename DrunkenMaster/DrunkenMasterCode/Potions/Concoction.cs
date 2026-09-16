using BaseLib.Cards.Variables;
using BaseLib.Utils;
using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace DrunkenMaster.DrunkenMasterCode.Potions;

/// <summary>
/// Spec §4. A sealed Brew. Its effects are the additive sum of its three ingredients, resolved to
/// their natural recipients (damage/debuffs → target, block/buffs → drinker). Targeted if any
/// ingredient is enemy-facing.
///
/// v0.1 STATUS: per-instance ingredient list lives in a SpireField (in-memory only; NOT saved).
/// Description is composed from the ingredients' `.brewText` entries via a DisplayVar.
/// TODO: SavedSpireField + ExtendedSaveTypes registration so the potion survives save/reload (spec §8 spike).
/// </summary>
public class Concoction : DrunkenMasterPotion
{
    /// <summary>Per-instance ingredient list. Copied when the model is cloned.</summary>
    public static readonly NotNullSpireField<PotionModel, List<IngredientCard>> Ingredients =
        new NotNullSpireField<PotionModel, List<IngredientCard>>(() => new List<IngredientCard>()).CopyOnClone();

    public override PotionRarity Rarity => PotionRarity.Token;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override bool CanBeGeneratedInCombat => false;

    private List<IngredientCard> MyIngredients => Ingredients[this];

    private static bool IsModifier(IngredientCard i) => i is Everclear or Seltzer or PunchBowl;
    private int Multiplier => 1 + MyIngredients.OfType<Everclear>().Sum(e => e.ExtraTriggers);
    private bool SplashEnemies => MyIngredients.Any(i => i is Seltzer);
    private bool SplashAllies => MyIngredients.Any(i => i is PunchBowl);
    private bool TargetsEnemies => MyIngredients.Any(i => i.TargetsEnemy);

    /// <summary>
    /// Composed description: the ingredient texts in the order first played (spec §4), duplicates collapsed
    /// into one scaled line, and Everclear's doubling already folded into the numbers.
    /// </summary>
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DisplayVar<Concoction>("Effects", p => p.MyIngredients.Count == 0
            ? "[i](empty)[/i]"
            : string.Join(" ", p.MyIngredients
                .GroupBy(i => (i.Id, i.IsUpgraded))
                .Select(g => IsModifier(g.First()) ? g.First().GetBrewEffectText(1) : g.First().GetBrewEffectText(g.Count() * p.Multiplier))))
    ];

    // Per-instance, not per-model (spec §4 Targeting). Seltzer turns a targeted brew into an all-enemies one.
    public override TargetType TargetType =>
        TargetsEnemies ? (SplashEnemies ? TargetType.AllEnemies : TargetType.AnyEnemy) : TargetType.AnyPlayer;

    public override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip];

    /// <summary>
    /// Concoctions are combat-scoped (2026-09-12): one still in a slot when combat ends becomes Dregs in
    /// the same slot. Dregs carry no state, which is what makes them save-safe.
    /// </summary>
    public override async Task AfterCombatEnd(CombatRoom room)
    {
        var owner = Owner;
        if (owner == null) return;
        int slot = owner.GetPotionSlotIndex(this);
        if (slot < 0) return;
        MainFile.Logger.Info($"Concoction in slot {slot} expired into Dregs.");
        await PotionCmd.Discard(this);
        await PotionCmd.TryToProcure(ModelDb.Potion<Dregs>().ToMutable(), owner, slot);
    }

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var drinker = Owner;
        var ingredients = MyIngredients;
        var combatState = drinker.Creature.CombatState;
        int multiplier = Multiplier;
        bool splashEnemies = SplashEnemies;
        bool splashAllies = SplashAllies;

        MainFile.Logger.Info($"Drinking Concoction: {string.Join(", ", ingredients.Select(i => i.Id.Entry + (i.IsUpgraded ? "+" : "")))} (x{multiplier}{(splashEnemies ? ", splash" : "")}{(splashAllies ? ", allies" : "")})");

        // Who gets the enemy-facing effects and who gets the self-facing ones.
        var enemies = new List<Creature>();
        if (TargetsEnemies)
        {
            if (splashEnemies && combatState != null) enemies.AddRange(combatState.HittableEnemies);
            else if (target != null) enemies.Add(target);
        }
        var drinkers = new List<Player> { drinker };
        if (splashAllies && combatState != null)
        {
            foreach (var ally in combatState.GetCreaturesOnSide(CombatSide.Player))
                if (ally.Player != null && ally.IsAlive && ally.Player != drinker) drinkers.Add(ally.Player);
        }

        foreach (var ingredient in ingredients)
        {
            if (IsModifier(ingredient)) continue;
            for (int n = 0; n < multiplier; n++)
            {
                if (ingredient.TargetsEnemy)
                {
                    foreach (var enemy in enemies.ToList())
                        if (enemy.IsAlive) await ingredient.ApplyBrewedEffect(choiceContext, drinker, enemy);
                }
                else
                {
                    foreach (var who in drinkers)
                        await ingredient.ApplyBrewedEffect(choiceContext, who, null);
                }
            }
        }

        // 2026-09-12: drinking no longer grants Intoxication by itself. Only Ethanol (an Ingredient) does.
    }
}
