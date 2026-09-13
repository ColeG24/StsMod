using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Uncommon;

/// <summary>
/// Uncommon (2026-09-13, was Common). 3 Energy. Gain 13 Block. Costs 1 less for each Ingredient you played this turn. Upgraded: 17 Block.
/// Cost tracking follows the base game's Stomp: catch up from combat history when the card enters
/// combat (drawn mid-turn after Ingredients were already played), then tick down on each Ingredient play.
/// </summary>
public class Bouncer() : DrunkenMasterCard(3, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(13, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(DrunkenMasterTips.Ingredient),
        HoverTipFactory.Static(DrunkenMasterTips.Brew)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(4m);

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card != this || IsClone) return Task.CompletedTask;
        int played = CombatManager.Instance.History.CardPlaysFinished.Count(e =>
            e.CardPlay.Card is IngredientCard && e.CardPlay.Player == Owner && e.HappenedThisTurn(CombatState));
        if (played > 0) EnergyCost.AddThisTurn(-played);
        return Task.CompletedTask;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner || cardPlay.Card is not IngredientCard) return Task.CompletedTask;
        EnergyCost.AddThisTurn(-1);
        return Task.CompletedTask;
    }
}
