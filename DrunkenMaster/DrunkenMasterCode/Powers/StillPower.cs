using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>
/// Draw Amount additional cards each turn. At the start of your turn, choose a card in your hand to transform into a
/// random Ingredient (Distill's mechanic: removed from combat, Ingredient added). 2026-09-18 rework; was "add Amount
/// random Ingredients at the start of your turn". The pick was random for a few hours; the user wants the choice.
/// Ingredients are not offered so the power cannot eat its own output; the pick cannot be skipped. The prompt runs in
/// the turn-start hook context, which is owned by this player, so it is safe in co-op (see DrunkenMasterBands.Blackout).
/// </summary>
public class StillPower : DrunkenMasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(DrunkenMasterTips.Ingredient)];

    public override decimal ModifyHandDraw(Player player, decimal count) => player == Owner.Player ? count + Amount : count;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        if (!PileType.Hand.GetPile(player).Cards.Any(c => c is not IngredientCard)) return;
        Flash();
        var prompt = new LocString("powers", $"{Id.Entry}.selectionScreenPrompt");
        var picked = (await CardSelectCmd.FromHand(choiceContext, player, new CardSelectorPrefs(prompt, 1), c => c is not IngredientCard, this)).FirstOrDefault();
        if (picked == null) return;
        await CardPileCmd.RemoveFromCombat(picked);
        await BrewSystem.AddRandomIngredientsToHand(player, 1);
    }
}
