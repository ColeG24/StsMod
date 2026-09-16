using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Rare;

/// <summary>
/// Rare Attack (2026-09-15), 0 Energy. Can only be played by a Blackout. Deal 50 damage to ALL enemies. Upgraded: 65.
/// Implemented as <see cref="IsPlayable"/> false (BlockedByCardLogic, so it greys out in hand) rather than the Unplayable
/// keyword: CardCmd.AutoPlay moves keyword-Unplayable cards aside without playing them, but only checks Hook.ShouldPlay
/// otherwise, so the Blackout auto-play from the draw pile goes through. Sits in hand as a dead draw until then.
/// </summary>
public class ClearTheBar() : DrunkenMasterCard(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(50, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        IntoxicationResource.Tip,
        IntoxicationResource.BandTip(IntoxicationResource.Band.Blackout)
    ];

    protected override bool IsPlayable => false;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_heavy_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(15m);
}
