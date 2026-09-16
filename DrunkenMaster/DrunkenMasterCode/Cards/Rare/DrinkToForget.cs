using DrunkenMaster.DrunkenMasterCode.Resources;
using DrunkenMaster.DrunkenMasterCode.Tips;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Rare;

/// <summary>
/// Rare Skill (2026-09-15), 0 Energy + 5 Intoxication, Exhaust. Discard your hand. Draw 5 cards. Upgraded: draw 7.
/// Poised is explicit here because the keyword override replaces the base class list.
/// </summary>
public class DrinkToForget : DrunkenMasterCard
{
    public const int IntoxicationCost = 5;
    public DrinkToForget() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
        SetIntoxicationCost(IntoxicationCost);
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [DrunkenMasterKeywords.Poised, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(5),
        new DynamicVar("IntoxicationCost", IntoxicationCost)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hand = PileType.Hand.GetPile(Owner).Cards.ToList();
        await CardCmd.DiscardAndDraw(choiceContext, hand, DynamicVars.Cards.IntValue);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(2m);
}
