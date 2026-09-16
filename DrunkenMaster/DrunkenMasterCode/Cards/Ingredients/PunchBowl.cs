using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;

/// <summary>
/// Co-op only (2026-09-15 night). Contributes nothing itself; the Concoction's self effects (Block, cards, Energy) reach
/// every living ally as well as the drinker. Ethanol is the exception: it only lands on Drunken Masters. Only in the random
/// pool when the run has more than one player (<see cref="Brew.BrewSystem.IngredientPoolFor"/>), so a singleplayer run
/// never sees it. Upgraded: Retain instead of Ethereal, like Everclear+ and Seltzer+.
/// </summary>
public class PunchBowl : IngredientCard
{
    public override bool TargetsEnemy => false;
    public override Task ApplyBrewedEffect(PlayerChoiceContext choiceContext, Player drinker, Creature? enemyTarget) => Task.CompletedTask;
    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
        AddKeyword(CardKeyword.Retain);
    }
}
