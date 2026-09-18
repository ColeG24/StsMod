using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Resources;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Uncommon;

/// <summary>
/// Uncommon Skill, 1 Energy (2026-09-18). Double your Intoxication. Add an Everclear into your hand. Upgraded: the
/// Everclear is Upgraded (Retain instead of Ethereal). Doubling goes through GainAsync, so it is capped at 12, crosses
/// bands normally and can walk you straight into a Blackout; at 0 it adds nothing. Name is mine.
/// </summary>
public class DoubleDown() : DrunkenMasterCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        IntoxicationResource.Tip,
        HoverTipFactory.FromCard<Everclear>(IsUpgraded),
        HoverTipFactory.Static(DrunkenMasterTips.Brew)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int current = IntoxicationResource.AmountOf(Owner);
        if (current > 0) await IntoxicationResource.GainAsync(choiceContext, Owner, current);
        if (CombatState == null) return;
        var everclear = BrewSystem.CreateIngredient(Owner, ModelDb.Card<Everclear>(), upgraded: IsUpgraded);
        await CardPileCmd.AddGeneratedCardToCombat(everclear, PileType.Hand, Owner);
    }

    protected override void OnUpgrade() { }
}
