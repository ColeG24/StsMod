using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>While Sober, whenever you play an Ingredient, draw Amount cards.</summary>
public class SteadyHandsPower : DrunkenMasterPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip, IntoxicationResource.BandTip(IntoxicationResource.Band.Sober)];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card is not IngredientCard || cardPlay.Card.Owner != Owner.Player) return;
        if (IntoxicationResource.BandFor(IntoxicationResource.AmountOf(Owner.Player)) != IntoxicationResource.Band.Sober) return;
        Flash();
        await CardPileCmd.Draw(choiceContext, Amount, Owner.Player);
    }
}
