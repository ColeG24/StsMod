using BaseLib.Abstracts;
using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Rare;

/// <summary>1 Energy, X Intoxication (spends all). Deal 6 damage once per Intoxication spent. Upgraded: 8. Needs at least 1.</summary>
public class LastRound : DrunkenMasterCard
{
    public LastRound() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        SetIntoxicationXCost();
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        int spent = Math.Max(0, CustomResources<IntoxicationResource>.AmountSpent(cardPlay));
        if (spent <= 0) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(spent)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_heavy_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}
