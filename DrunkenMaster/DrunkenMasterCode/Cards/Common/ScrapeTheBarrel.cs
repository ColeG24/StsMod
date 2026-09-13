using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Common;

/// <summary>
/// 2 Energy. Deal 4 damage 4 times. Put 1 random Ingredient from your exhaust pile into your hand. Upgraded: 2 Ingredients.
/// Ingredients Exhaust when brewed or when they fizzle at end of turn, so the barrel fills up as the fight goes on.
/// </summary>
public class ScrapeTheBarrel() : DrunkenMasterCard(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    public const string IngredientsKey = "Ingredients";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4, ValueProp.Move),
        new RepeatVar(4),
        new DynamicVar(IngredientsKey, 1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(DrunkenMasterTips.Ingredient),
        HoverTipFactory.Static(DrunkenMasterTips.Brew)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .Execute(choiceContext);

        var pile = PileType.Exhaust.GetPile(Owner);
        var rng = Owner.RunState.Rng.CombatCardGeneration;
        for (int i = 0; i < DynamicVars[IngredientsKey].IntValue; i++)
        {
            var candidates = pile.Cards.OfType<IngredientCard>().ToList();
            if (candidates.Count == 0) break;
            var pick = candidates[rng.NextInt(candidates.Count)];
            await CardPileCmd.Add(pick, PileType.Hand);
        }
    }

    protected override void OnUpgrade() => DynamicVars[IngredientsKey].UpgradeValueBy(1m);
}
