using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Uncommon;

/// <summary>
/// Uncommon Attack (2026-09-15; was Common), 1 Energy. Deal 10 damage. Draw 1 card for each Concoction you brewed this
/// turn. Upgraded: 13 damage, draw 2 per Concoction. Until 2026-09-15: 7 (10) damage, draw 1 per Ingredient in the Brew.
/// Rewards playing Ingredients before attacks; a second seal in one turn needs a Shot Glass pot or a hand full of Ingredients.
/// </summary>
public class StirThePot() : DrunkenMasterCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10, ValueProp.Move),
        new CardsVar(1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(DrunkenMasterTips.Brew),
        HoverTipFactory.Static(DrunkenMasterTips.Ingredient)
    ];

    protected override bool ShouldGlowGoldInternal => BrewSystem.SealsThisTurn(Owner) > 0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .Execute(choiceContext);
        int seals = BrewSystem.SealsThisTurn(Owner);
        if (seals > 0) await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue * seals, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
