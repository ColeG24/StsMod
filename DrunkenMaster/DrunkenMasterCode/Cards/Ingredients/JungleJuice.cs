using DrunkenMaster.DrunkenMasterCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;

/// <summary>Contributes: Apply 1 Confusion. Upgraded: 2.</summary>
public class JungleJuice : IngredientCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<ConfusionPower>(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        base.ExtraHoverTips.Append(HoverTipFactory.FromPower<ConfusionPower>());
    public override bool TargetsEnemy => true;

    public override async Task ApplyBrewedEffect(PlayerChoiceContext choiceContext, Player drinker, Creature? enemyTarget)
    {
        if (enemyTarget == null) return;
        await PowerCmd.Apply<ConfusionPower>(choiceContext, enemyTarget, DynamicVars[nameof(ConfusionPower)].BaseValue, drinker.Creature, null);
    }
    protected override void OnUpgrade() => DynamicVars[nameof(ConfusionPower)].UpgradeValueBy(1m);
}
