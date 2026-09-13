using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Powers;
using DrunkenMaster.DrunkenMasterCode.Resources;
using DrunkenMaster.DrunkenMasterCode.Tips;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Common;

/// <summary>1 Energy, Exhaust. Gain 4 Block. Put an Ingredient from your exhaust pile into your hand. Upgraded: 7 Block, no Exhaust.</summary>
public class Leftovers() : DrunkenMasterCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(4, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(DrunkenMasterTips.Ingredient)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        var pile = PileType.Exhaust.GetPile(Owner);
        if (!pile.Cards.Any(c => c is IngredientCard)) return;
        var picked = (await CardSelectCmd.FromCombatPile(choiceContext, pile, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1), c => c is IngredientCard)).FirstOrDefault();
        if (picked != null) await CardPileCmd.Add(picked, PileType.Hand);
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
