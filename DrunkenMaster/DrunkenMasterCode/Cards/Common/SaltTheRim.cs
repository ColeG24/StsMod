using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models.Powers;
using DrunkenMaster.DrunkenMasterCode.Powers;
using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Common;

/// <summary>Common Attack (2026-09-14, was Uncommon), 0 Energy + 1 Intoxication. Deal 3 damage. Apply 1 Vulnerable. Upgraded: 5 / 2.</summary>
public class SaltTheRim : DrunkenMasterCard
{
    public const int IntoxicationCost = 1;

    public SaltTheRim() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        CustomResources<IntoxicationResource>.SetCanonicalCost(this, IntoxicationCost);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3, ValueProp.Move),
        new PowerVar<VulnerablePower>(1),
        new DynamicVar("IntoxicationCost", IntoxicationCost)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip, HoverTipFactory.FromPower<VulnerablePower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .Execute(choiceContext);
        if (cardPlay.Target.IsAlive)
            await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, DynamicVars[nameof(VulnerablePower)].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars[nameof(VulnerablePower)].UpgradeValueBy(1m);
    }
}
