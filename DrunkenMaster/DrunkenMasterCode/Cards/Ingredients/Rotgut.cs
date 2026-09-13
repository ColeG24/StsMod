using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;

/// <summary>Contributes: Deal 5 damage. Upgraded: 8.</summary>
public class Rotgut : IngredientCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5, ValueProp.Unpowered)];

    public override bool TargetsEnemy => true;

    public override async Task ApplyBrewedEffect(PlayerChoiceContext choiceContext, Player drinker, Creature? enemyTarget)
    {
        if (enemyTarget == null) return;
        var damage = DynamicVars.Damage;
        await CreatureCmd.Damage(choiceContext, enemyTarget, damage.BaseValue, damage.Props, drinker.Creature, null, null);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}
