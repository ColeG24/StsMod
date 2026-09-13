using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Uncommon;

/// <summary>
/// Uncommon Skill, 0 cost, Exhaust. Gain 1 Energy for each Ingredient in your hand. Upgraded: gains Retain.
/// Counts the hand at the moment of play.
/// </summary>
public class LineEmUp() : DrunkenMasterCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        EnergyHoverTip,
        HoverTipFactory.Static(DrunkenMasterTips.Ingredient)
    ];

    protected override bool ShouldGlowGoldInternal => CountIngredientsInHand() > 0;

    private int CountIngredientsInHand() =>
        PileType.Hand.GetPile(Owner).Cards.OfType<IngredientCard>().Count();

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal energy = DynamicVars.Energy.BaseValue * CountIngredientsInHand();
        if (energy > 0)
        {
            await PlayerCmd.GainEnergy(energy, Owner);
        }
    }

    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
