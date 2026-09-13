using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;

/// <summary>Contributes: Apply 1 Weak. Upgraded: 2.</summary>
public class Wormwood : IngredientCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<WeakPower>(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        base.ExtraHoverTips.Append(HoverTipFactory.FromPower<WeakPower>());
    public override bool TargetsEnemy => true;

    public override async Task ApplyBrewedEffect(PlayerChoiceContext choiceContext, Player drinker, Creature? enemyTarget)
    {
        if (enemyTarget == null) return;
        await PowerCmd.Apply<WeakPower>(choiceContext, enemyTarget, DynamicVars[nameof(WeakPower)].BaseValue, drinker.Creature, null);
    }
    protected override void OnUpgrade() => DynamicVars[nameof(WeakPower)].UpgradeValueBy(1m);
}
