using DrunkenMaster.DrunkenMasterCode.Powers;
using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Rare;

/// <summary>
/// Rare Skill, 0 Energy, Retain, Exhaust (2026-09-16). If you are Hungover, gain 2 Energy. Draw 2 cards. Upgraded: draw 3.
/// The Blackout deck's recovery turn: Retain carries it through the Blackout discard, and the Energy pays back what
/// Hungover took. Hungover is not removed, since its Energy and draw costs are already paid by the time a card can be played.
/// </summary>
public class BreakfastBeer() : DrunkenMasterCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(2),
        new CardsVar(2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<HungoverPower>(),
        IntoxicationResource.BandTip(IntoxicationResource.Band.Blackout)
    ];

    private bool IsHungover => Owner.Creature?.HasPower<HungoverPower>() == true;
    protected override bool ShouldGlowGoldInternal => IsHungover;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (IsHungover)
        {
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        }
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}
