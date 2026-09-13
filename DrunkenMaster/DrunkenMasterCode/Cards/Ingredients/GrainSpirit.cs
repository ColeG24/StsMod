using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;

/// <summary>Contributes: Gain 1 Energy. Upgraded: 2.</summary>
public class GrainSpirit : IngredientCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    public override bool TargetsEnemy => false;

    public override async Task ApplyBrewedEffect(PlayerChoiceContext choiceContext, Player drinker, Creature? enemyTarget)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, drinker);
    }
    protected override void OnUpgrade() => DynamicVars.Energy.UpgradeValueBy(1m);
}
