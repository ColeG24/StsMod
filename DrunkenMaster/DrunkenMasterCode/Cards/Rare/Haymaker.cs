using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Rare;

/// <summary>
/// Rare Attack (2026-09-15), 1 Energy. Deal 8 damage. Deals triple damage while Drunk (or Blackout). Upgraded: 11
/// (the upgrade was not specified; +3 is my pick). Calculated as 0 + 8 x (Drunk ? 3 : 1) so the card preview shows 24 while Drunk.
/// </summary>
public class Haymaker() : DrunkenMasterCard(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    public const int DrunkMultiplier = 3;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(0),
        new ExtraDamageVar(8),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(Multiplier),
        new DynamicVar("Multiplier", DrunkMultiplier)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        IntoxicationResource.Tip,
        IntoxicationResource.BandTip(IntoxicationResource.Band.Drunk)
    ];

    private static bool IsDrunk(CardModel card) =>
        IntoxicationResource.BandFor(IntoxicationResource.AmountOf(card.Owner)) >= IntoxicationResource.Band.Drunk;

    private static decimal Multiplier(CardModel card, Creature? _) => IsDrunk(card) ? DrunkMultiplier : 1;

    protected override bool ShouldGlowGoldInternal => IsDrunk(this);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_heavy_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.ExtraDamage.UpgradeValueBy(3m);
}
