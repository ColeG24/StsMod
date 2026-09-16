using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Rare;

/// <summary>
/// Rare Skill (2026-09-15), 2 Energy, Exhaust. Transform any number of cards in your hand into Muddles. Upgraded: the Muddles are
/// Upgraded (Distill / Restock convention). "Transform" is Distill's: the chosen cards leave combat and a fresh Muddle
/// lands in hand for each. Playing it with an empty hand does nothing.
/// </summary>
public class MuddleThrough() : DrunkenMasterCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<Muddle>(IsUpgraded),
        HoverTipFactory.Static(DrunkenMasterTips.Brew)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hand = PileType.Hand.GetPile(Owner);
        if (hand.Cards.Count == 0 || CombatState == null) return;
        var picked = (await CardSelectCmd.FromHand(choiceContext, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 0, hand.Cards.Count), null, this)).ToList();
        foreach (var card in picked)
        {
            await CardPileCmd.RemoveFromCombat(card);
            var muddle = BrewSystem.CreateIngredient(Owner, ModelDb.Card<Muddle>(), upgraded: IsUpgraded);
            await CardPileCmd.AddGeneratedCardToCombat(muddle, PileType.Hand, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        // The upgrade is "the Muddles come Upgraded"; see OnPlay and the {IfUpgraded} text.
    }
}
