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

/// <summary>1 Energy. Choose a card in your hand: it is removed from combat and replaced by a random Ingredient. Gain 2 Intoxication (2026-09-14). Upgraded: costs 0.</summary>
public class Distill() : DrunkenMasterCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public const string IntoxicationKey = "Intoxication";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(IntoxicationKey, 2)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(DrunkenMasterTips.Ingredient), IntoxicationResource.Tip];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hand = PileType.Hand.GetPile(Owner);
        if (hand.Cards.Count > 0)
        {
            var picked = (await CardSelectCmd.FromHand(choiceContext, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1), null, this)).FirstOrDefault();
            if (picked != null)
            {
                await CardPileCmd.RemoveFromCombat(picked);
                await BrewSystem.AddRandomIngredientsToHand(Owner, 1);
            }
        }
        await IntoxicationResource.GainAsync(choiceContext, Owner, DynamicVars[IntoxicationKey].IntValue);
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
