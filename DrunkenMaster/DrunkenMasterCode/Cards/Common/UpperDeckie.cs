using DrunkenMaster.DrunkenMasterCode.Resources;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Common;

/// <summary>
/// Common Attack, 0 Energy. Deal 3 damage (Upgraded: 5). After it resolves, every Upper Deckie in this combat (this copy
/// included) deals damage equal to your current Intoxication more. The buff is combat-scoped and lands after the hit,
/// so the play that grants it never benefits from it. Same shape as the game's Claw, including the downgrade guard.
/// </summary>
public class UpperDeckie() : DrunkenMasterCard(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    private decimal _extraDamageFromPlays;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3, ValueProp.Move)];

    /// <summary>Poised (2026-09-14): a 0-cost card that scales with Intoxication should not get re-rolled to 3 while Drunk.</summary>
    public override IEnumerable<CardKeyword> CanonicalKeywords => [DrunkenMasterKeywords.Poised];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .Execute(choiceContext);

        int intoxication = IntoxicationResource.AmountOf(Owner);
        if (intoxication <= 0) return;
        var combatState = Owner.PlayerCombatState;
        if (combatState == null) return;
        foreach (var copy in combatState.AllCards.OfType<UpperDeckie>().ToList())
        {
            copy.BuffFromPlay(intoxication);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);

    // Upgrading/downgrading rebuilds the vars from canonical; put the combat buff back (Claw does the same).
    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        DynamicVars.Damage.BaseValue += _extraDamageFromPlays;
    }

    private void BuffFromPlay(decimal extraDamage)
    {
        DynamicVars.Damage.BaseValue += extraDamage;
        _extraDamageFromPlays += extraDamage;
    }
}
