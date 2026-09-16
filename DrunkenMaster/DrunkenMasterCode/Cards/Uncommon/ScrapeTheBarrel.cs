using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Uncommon;

/// <summary>
/// Uncommon Skill, 1 Energy, Exhaust (2026-09-16 rework; was a 2-Energy 4x4 Attack that pulled 1 (2) random Ingredients).
/// Choose up to 3 Ingredients in your exhaust pile and put them into your hand. Upgraded: Retain (so it can be held until
/// the pile is worth scraping). Exhaust keeps it a one-shot: without it the card refilled the pot every deck cycle.
/// Ingredients Exhaust when brewed or when they fizzle at end of turn, so the barrel fills up as the fight goes on.
/// Leftovers is the 1-Ingredient Common; Line 'Em Up / Kitchen Sink / Bouncer are the payoffs for three plays at once.
/// </summary>
public class ScrapeTheBarrel() : DrunkenMasterCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public const string IngredientsKey = "Ingredients";

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(IngredientsKey, 3)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(DrunkenMasterTips.Ingredient),
        HoverTipFactory.Static(DrunkenMasterTips.Brew)
    ];

    protected override bool ShouldGlowGoldInternal => PileType.Exhaust.GetPile(Owner).Cards.Any(c => c is IngredientCard);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var pile = PileType.Exhaust.GetPile(Owner);
        int available = pile.Cards.Count(c => c is IngredientCard);
        if (available == 0) return;
        int max = Math.Min(available, DynamicVars[IngredientsKey].IntValue);
        var picked = await CardSelectCmd.FromCombatPile(choiceContext, pile, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 0, max), c => c is IngredientCard);
        foreach (var card in picked)
        {
            await CardPileCmd.Add(card, PileType.Hand);
        }
    }

    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
