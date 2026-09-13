using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;

/// <summary>
/// Contributes nothing itself; every OTHER Ingredient in the Concoction triggers ExtraTriggers more times
/// (1 = doubled). Upgraded: 2 = tripled. Multiple Everclears add up.
/// </summary>
public class Everclear : IngredientCard
{
    public const string ExtraTriggersKey = "ExtraTriggers";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(ExtraTriggersKey, 1)];
    public int ExtraTriggers => DynamicVars[ExtraTriggersKey].IntValue;
    public override bool TargetsEnemy => false;
    public override Task ApplyBrewedEffect(PlayerChoiceContext choiceContext, Player drinker, Creature? enemyTarget) => Task.CompletedTask;
    protected override void OnUpgrade() => DynamicVars[ExtraTriggersKey].UpgradeValueBy(1m);
}
