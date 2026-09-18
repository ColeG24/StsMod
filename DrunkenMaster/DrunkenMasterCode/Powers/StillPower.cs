using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>
/// Draw Amount additional cards each turn. At the start of your turn, transform a random non-Ingredient card in your
/// hand into a random Ingredient (Distill's mechanic without the prompt: removed from combat, Ingredient added).
/// 2026-09-18 rework; was "add Amount random Ingredients at the start of your turn". Ingredients are never picked so
/// the power cannot eat its own output; a hand of nothing but Ingredients transforms nothing.
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
        var candidates = PileType.Hand.GetPile(player).Cards.Where(c => c is not IngredientCard).ToList();
        if (candidates.Count == 0) return;
        Flash();
        var picked = player.RunState.Rng.CombatCardGeneration.NextItem(candidates);
        if (picked == null) return;
        await CardPileCmd.RemoveFromCombat(picked);
        await BrewSystem.AddRandomIngredientsToHand(player, 1);
    }
}
