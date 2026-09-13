using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Common;

/// <summary>
/// Gain 2 Intoxication. Deal 4 damage. Deals 2 additional damage for each Intoxication.
/// Upgraded: base damage 6 and gain 3 Intoxication.
/// 2026-09-13: Intoxication 1 -> 2 so the card is net positive against the 1/turn decay (spec Q6).
/// Common since 2026-09-12 (was the starting Intoxication source; a new starter takes that job).
///
/// Spec §3 says "gain Intoxication first, then compute damage". To keep the card preview honest
/// (a first copy should read 8, not 4) the damage formula counts the Intoxication this card is
/// about to grant, then the attack resolves, then the power is applied. The observable result is
/// identical to gain-then-hit.
/// </summary>
public class LiquidCourage() : DrunkenMasterCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(IntoxicationKey, 2),
        new CalculationBaseVar(4),
        new ExtraDamageVar(2),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CountIntoxicationAfterPlay)
    ];

    public const string IntoxicationKey = "Intoxication";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip];

    private static decimal CountIntoxicationAfterPlay(CardModel card, Creature? _)
    {
        int current = IntoxicationResource.AmountOf(card.Owner);
        int granted = card.DynamicVars[IntoxicationKey].IntValue;
        return current + granted;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .Execute(choiceContext);

        await IntoxicationResource.GainAsync(choiceContext, Owner, DynamicVars[IntoxicationKey].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(2m);
        DynamicVars[IntoxicationKey].UpgradeValueBy(1m);
    }
}
