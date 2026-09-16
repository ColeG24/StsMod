using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Uncommon;

/// <summary>
/// Uncommon Attack, 1 Energy. Deal 8 damage. Hits twice if you are Tipsy or above. Upgraded: 10.
/// 2026-09-15: renamed from Drunken Fist and given the Strike tag so it counts for Strike synergies (user wants to build on those).
/// 2026-09-13: was Rare and hit once more per band above Sober (judged confusing).
/// </summary>
public class DrunkenStrike() : DrunkenMasterCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip, IntoxicationResource.BandTip(IntoxicationResource.Band.Tipsy)];

    private bool TipsyOrAbove => IntoxicationResource.BandFor(IntoxicationResource.AmountOf(Owner)) >= IntoxicationResource.Band.Tipsy;
    protected override bool ShouldGlowGoldInternal => TipsyOrAbove;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitCount(TipsyOrAbove ? 2 : 1)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}
