using DrunkenMaster.DrunkenMasterCode.Resources;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Uncommon;

/// <summary>
/// Uncommon Skill, 1 Energy (2026-09-18 rework; was an Attack: 10 (13) damage + draw per Concoction brewed this turn).
/// Gain 2 Intoxication. Add a random Poised card from your draw pile into your hand. Upgraded: 3 Intoxication and you
/// choose the card. Poised cards are the Intoxication-cost cards (plus Upper Deckie / Chug), so this fetches the
/// thing the Intoxication is for. Nothing happens on a draw pile with no Poised card.
/// </summary>
public class StirThePot() : DrunkenMasterCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public const string IntoxicationKey = "Intoxication";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(IntoxicationKey, 2)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        IntoxicationResource.Tip,
        HoverTipFactory.FromKeyword(DrunkenMasterKeywords.Poised)
    ];

    private static bool IsPoised(CardModel card) => card.Keywords.Contains(DrunkenMasterKeywords.Poised);

    protected override bool ShouldGlowGoldInternal => PileType.Draw.GetPile(Owner).Cards.Any(IsPoised);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await IntoxicationResource.GainAsync(choiceContext, Owner, DynamicVars[IntoxicationKey].IntValue);
        var pile = PileType.Draw.GetPile(Owner);
        var candidates = pile.Cards.Where(IsPoised).ToList();
        if (candidates.Count == 0) return;
        CardModel? picked = IsUpgraded
            ? (await CardSelectCmd.FromCombatPile(choiceContext, pile, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1), IsPoised)).FirstOrDefault()
            : Owner.RunState.Rng.CombatCardGeneration.NextItem(candidates);
        if (picked != null) await CardPileCmd.Add(picked, PileType.Hand);
    }

    protected override void OnUpgrade() => DynamicVars[IntoxicationKey].UpgradeValueBy(1m);
}
