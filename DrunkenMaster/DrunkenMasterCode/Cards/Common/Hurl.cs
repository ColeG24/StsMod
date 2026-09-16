using BaseLib.Abstracts;
using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Common;

/// <summary>0 Energy + 3 Intoxication. Deal 14 damage. Upgraded: 18. Unplayable below 3 Intoxication.</summary>
public class Hurl : DrunkenMasterCard
{
    public const int IntoxicationCost = 3;

    public Hurl() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
        SetIntoxicationCost(IntoxicationCost);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(14, ValueProp.Move),
        new DynamicVar("IntoxicationCost", IntoxicationCost)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_heavy_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}
