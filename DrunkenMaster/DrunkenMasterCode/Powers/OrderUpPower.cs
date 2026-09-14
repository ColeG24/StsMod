using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>
/// Next turn, add Amount Grain Spirits into your hand, then the power removes itself (Nightcap-style one-shot).
/// A fixed Ingredient since 2026-09-14 (was random): the Brew-flavored Charge Battery, so the promise is always Energy.
/// </summary>
public class OrderUpPower : DrunkenMasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<GrainSpirit>()];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        Flash();
        var combatState = player.Creature.CombatState;
        if (combatState != null)
        {
            var cards = new List<CardModel>(Amount);
            for (int i = 0; i < Amount; i++) cards.Add(BrewSystem.CreateIngredient(player, ModelDb.Card<GrainSpirit>()));
            await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, player);
        }
        await PowerCmd.Remove(this);
    }
}
