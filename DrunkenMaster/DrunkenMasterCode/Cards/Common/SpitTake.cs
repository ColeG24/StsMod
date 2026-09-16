using DrunkenMaster.DrunkenMasterCode.Resources;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Common;

/// <summary>
/// Common Attack (2026-09-15), 0 Energy + 2 Intoxication. Deal 9 damage to ALL enemies. Upgraded: 12.
/// The Intoxication-cost AoE next to Barstool Swing's Energy one. Poised comes with the Intoxication cost.
/// </summary>
public class SpitTake : DrunkenMasterCard
{
    public const int IntoxicationCost = 2;
    public SpitTake() : base(0, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
        SetIntoxicationCost(IntoxicationCost);
    }
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9, ValueProp.Move),
        new DynamicVar("IntoxicationCost", IntoxicationCost)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .Execute(choiceContext);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}
