using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;

/// <summary>
/// Contributes: Gain 2 Intoxication (Upgraded: 3). Since 2026-09-12 this is the ONLY default way drinking makes you
/// drunk; Concoctions grant nothing on their own. Getting drunk is a brewing decision.
/// </summary>
public class Ethanol : IngredientCard
{
    public const string IntoxicationKey = "Intoxication";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(IntoxicationKey, 2)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [.. base.ExtraHoverTips, IntoxicationResource.Tip];
    public override bool TargetsEnemy => false;

    public override async Task ApplyBrewedEffect(PlayerChoiceContext choiceContext, Player drinker, Creature? enemyTarget)
    {
        await IntoxicationResource.GainAsync(choiceContext, drinker, DynamicVars[IntoxicationKey].IntValue);
    }
    protected override void OnUpgrade() => DynamicVars[IntoxicationKey].UpgradeValueBy(1m);
}
