using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;

/// <summary>Contributes: Apply 1 Vulnerable. Upgraded: 2.</summary>
public class Bitters : IngredientCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<VulnerablePower>(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        base.ExtraHoverTips.Append(HoverTipFactory.FromPower<VulnerablePower>());

    public override bool TargetsEnemy => true;

    public override async Task ApplyBrewedEffect(PlayerChoiceContext choiceContext, Player drinker, Creature? enemyTarget)
    {
        if (enemyTarget == null) return;
        await PowerCmd.Apply<VulnerablePower>(choiceContext, enemyTarget, DynamicVars.Vulnerable.BaseValue, drinker.Creature, null);
    }
    protected override void OnUpgrade() => DynamicVars.Vulnerable.UpgradeValueBy(1m);
}
