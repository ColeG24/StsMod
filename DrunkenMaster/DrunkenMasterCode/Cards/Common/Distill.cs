using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Powers;
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

/// <summary>
/// 1 Energy. Choose a card in your hand: it is removed from combat and replaced by a random Ingredient. Upgraded: the
/// Ingredient is Upgraded. 2026-09-15: the +2 Intoxication (added 2026-09-14) is gone and the upgrade no longer cuts the cost.
/// </summary>
public class Distill() : DrunkenMasterCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(DrunkenMasterTips.Ingredient), HoverTipFactory.Static(DrunkenMasterTips.Brew)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hand = PileType.Hand.GetPile(Owner);
        if (hand.Cards.Count > 0)
        {
            var picked = (await CardSelectCmd.FromHand(choiceContext, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1), null, this)).FirstOrDefault();
            if (picked != null)
            {
                await CardPileCmd.RemoveFromCombat(picked);
                await BrewSystem.AddRandomIngredientsToHand(Owner, 1, upgraded: IsUpgraded);
            }
        }
    }
    protected override void OnUpgrade()
    {
        // The upgrade is "the Ingredient comes Upgraded"; see OnPlay and the {IfUpgraded} text.
    }
}
