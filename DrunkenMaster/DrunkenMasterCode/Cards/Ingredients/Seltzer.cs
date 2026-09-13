using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;

/// <summary>
/// Contributes nothing itself; the Concoction's enemy effects hit ALL enemies.
/// Upgraded: its self effects reach every ally too.
/// </summary>
public class Seltzer : IngredientCard
{
    public bool ReachesAllies => IsUpgraded;
    public override bool TargetsEnemy => false;
    public override Task ApplyBrewedEffect(PlayerChoiceContext choiceContext, Player drinker, Creature? enemyTarget) => Task.CompletedTask;
    protected override void OnUpgrade()
    {
        // Upgrade is the ally splash itself (see ReachesAllies); no numbers to bump.
    }
}
