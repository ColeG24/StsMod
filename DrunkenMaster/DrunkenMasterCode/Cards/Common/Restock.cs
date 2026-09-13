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

/// <summary>1 Energy. Add 2 random Ingredients into your hand. Upgraded: the 2 Ingredients are Upgraded.</summary>
public class Restock() : DrunkenMasterCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public const string IngredientsKey = "Ingredients";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(IngredientsKey, 2)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(DrunkenMasterTips.Ingredient),
        HoverTipFactory.Static(DrunkenMasterTips.Brew)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await BrewSystem.AddRandomIngredientsToHand(Owner, DynamicVars[IngredientsKey].IntValue, upgraded: IsUpgraded);
    }
    protected override void OnUpgrade()
    {
        // The upgrade is "the Ingredients come Upgraded"; see OnPlay and the {IfUpgraded} text.
    }
}
