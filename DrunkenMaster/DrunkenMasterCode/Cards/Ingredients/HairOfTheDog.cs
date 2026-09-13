using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;

/// <summary>Contributes: Draw 1 card. Upgraded: 2.</summary>
public class HairOfTheDog : IngredientCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    public override bool TargetsEnemy => false;

    public override async Task ApplyBrewedEffect(PlayerChoiceContext choiceContext, Player drinker, Creature? enemyTarget)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, drinker);
    }
    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}
