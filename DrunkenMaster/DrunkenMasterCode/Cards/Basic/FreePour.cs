using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Basic;

/// <summary>
/// Gain 6 Block. Add a random Ingredient into your hand.
/// Upgraded: Gain 9 Block. Choose 1 of 3 Ingredients instead. (Block 5/8 -> 6/9 on 2026-09-13.)
/// (2026-09-12: the choice moved to the upgrade so the base card stays quick; spec §9.4.)
/// </summary>
public class FreePour() : DrunkenMasterCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    public const int Picks = 1;
    public const int Offer = 3;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(DrunkenMasterTips.Ingredient),
        HoverTipFactory.Static(DrunkenMasterTips.Brew)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        if (IsUpgraded) await BrewSystem.ChooseIngredientsToHand(choiceContext, Owner, Picks, Offer);
        else await BrewSystem.AddRandomIngredientsToHand(Owner, Picks);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
        // Upgrade also swaps the random Ingredient for a 1-of-3 choice; see OnPlay.
    }
}
