using BaseLib.Abstracts;
using DrunkenMaster.DrunkenMasterCode.Powers;
using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Uncommon;

/// <summary>Uncommon Skill, 0 Energy + 3 Intoxication. Apply 3 Confusion to ALL enemies. Upgraded: 5.</summary>
public class SpinTheBottle : DrunkenMasterCard
{
    public const int IntoxicationCost = 3;

    public SpinTheBottle() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
    {
        CustomResources<IntoxicationResource>.SetCanonicalCost(this, IntoxicationCost);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ConfusionPower>(3),
        new DynamicVar("IntoxicationCost", IntoxicationCost)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip, HoverTipFactory.FromPower<ConfusionPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combatState = CombatState;
        if (combatState == null) return;
        foreach (var enemy in combatState.HittableEnemies.ToList())
        {
            if (!enemy.IsAlive) continue;
            await PowerCmd.Apply<ConfusionPower>(choiceContext, enemy, DynamicVars[nameof(ConfusionPower)].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade() => DynamicVars[nameof(ConfusionPower)].UpgradeValueBy(2m);
}
