using DrunkenMaster.DrunkenMasterCode.Powers;
using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Rare;

/// <summary>
/// Rare Power, 3 Energy, Ethereal (2026-09-18). Start each turn at 12 Intoxication: you wake in Blackout every turn,
/// so every turn ends with a Blackout (3 auto-plays, reset to 0, Hungover) and the next starts back at 12. Upgraded:
/// loses Ethereal. The floor is applied by <see cref="IntoxicationResource.ApplyTurnStart"/> through
/// <see cref="IntoxicationResource.ITurnStartFloor"/>, so the dial's forecast shows it too.
/// </summary>
public class BlackoutForm() : DrunkenMasterCard(3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<BlackoutFormPower>(),
        IntoxicationResource.Tip,
        IntoxicationResource.BandTip(IntoxicationResource.Band.Blackout),
        HoverTipFactory.FromPower<HungoverPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        NPowerUpVfx.CreateNormal(Owner.Creature);
        await PowerCmd.Apply<BlackoutFormPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Ethereal);
}
