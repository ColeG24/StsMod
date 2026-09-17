using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Relics;

/// <summary>Uncommon. Whenever you play an Ingredient, deal 2 damage to ALL enemies (2026-09-17; was 1). Unpowered like Moonshiner.</summary>
public class Muddler : DrunkenMasterRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(2, ValueProp.Unpowered)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(DrunkenMasterTips.Ingredient)];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card is not IngredientCard || cardPlay.Card.Owner != Owner) return;
        var combatState = Owner.Creature.CombatState;
        if (combatState == null) return;
        Flash();
        var damage = DynamicVars.Damage;
        foreach (var enemy in combatState.HittableEnemies.ToList())
        {
            if (!enemy.IsAlive) continue;
            await CreatureCmd.Damage(choiceContext, enemy, damage.BaseValue, damage.Props, Owner.Creature);
        }
    }
}
