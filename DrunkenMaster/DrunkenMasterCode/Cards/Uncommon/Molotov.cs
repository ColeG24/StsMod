using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Uncommon;

/// <summary>2 Energy. Gain 3 Intoxication, then deal 14 damage to ALL enemies. Upgraded: 18 (2026-09-17; was 12 (16)). Uncommon since 2026-09-12 (was Rare with Exhaust).</summary>
public class Molotov() : DrunkenMasterCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
{

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(14, ValueProp.Move),
        new DynamicVar("Intoxication", 3)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Intoxication first so the hit benefits from the band you just entered (Tipsy: +2 damage).
        await IntoxicationResource.GainAsync(choiceContext, Owner, DynamicVars["Intoxication"].IntValue);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_heavy_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}
