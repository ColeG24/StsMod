using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Potions;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Uncommon;

/// <summary>
/// Uncommon Attack, 2 Energy (2026-09-14; was Common, 1 Energy, 10/12). Deal 13 damage. Add a Hair of the Dog into your
/// hand. Costs 1 less each time your Brew seals into a Concoction, until played. Upgraded: 14 damage and the Hair of
/// the Dog is Upgraded.
///
/// The discount is an until-played local cost modifier (survives end of turn, cleared when the card is played), fed by
/// <see cref="BrewSystem.IBrewSealedListener"/>: BrewSystem.TrySeal now notifies every card in the owner's combat, not
/// only powers. No history catch-up: a copy generated mid-combat starts at full cost.
/// </summary>
public class WakeUpCall() : DrunkenMasterCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy), BrewSystem.IBrewSealedListener
{
    public const string DiscountKey = "Discount";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(13, ValueProp.Move),
        new DynamicVar(DiscountKey, 1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<HairOfTheDog>(IsUpgraded),
        HoverTipFactory.Static(DrunkenMasterTips.Brew)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .Execute(choiceContext);
        if (CombatState == null) return;
        var hair = BrewSystem.CreateIngredient(Owner, ModelDb.Card<HairOfTheDog>(), upgraded: IsUpgraded);
        await CardPileCmd.AddGeneratedCardToCombat(hair, PileType.Hand, Owner);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(1m);

    public Task OnBrewSealed(Player owner, Concoction potion)
    {
        if (owner == Owner) EnergyCost.AddUntilPlayed(-DynamicVars[DiscountKey].IntValue);
        return Task.CompletedTask;
    }
}
