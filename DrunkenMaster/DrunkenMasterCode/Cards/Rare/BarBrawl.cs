using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Rare;

/// <summary>Rare Attack (2026-09-15 late; was Uncommon at 22/27), 3 Energy. Deal 23 damage. Add 2 Muddles into your hand. Upgraded: 28 damage (the Muddles stay unupgraded).</summary>
public class BarBrawl() : DrunkenMasterCard(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    public const string MuddlesKey = "Muddles";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(23, ValueProp.Move),
        new DynamicVar(MuddlesKey, 2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<Muddle>(),
        HoverTipFactory.Static(DrunkenMasterTips.Brew)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_heavy_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
        if (CombatState == null) return;
        for (int i = 0; i < DynamicVars[MuddlesKey].IntValue; i++)
        {
            var muddle = BrewSystem.CreateIngredient(Owner, ModelDb.Card<Muddle>());
            await CardPileCmd.AddGeneratedCardToCombat(muddle, PileType.Hand, Owner);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(5m);
}
