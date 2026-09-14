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

namespace DrunkenMaster.DrunkenMasterCode.Cards.Common;

/// <summary>
/// 1 Energy. Deal 8 damage. Add a Bitters into your hand. Upgraded: 11 damage and the Bitters is Upgraded.
/// A Common attack that brews (the Ball Lightning slot): damage now, Vulnerable in the pot.
/// </summary>
public class SourPunch() : DrunkenMasterCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<Bitters>(IsUpgraded),
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
        var bitters = BrewSystem.CreateIngredient(Owner, ModelDb.Card<Bitters>(), upgraded: IsUpgraded);
        await CardPileCmd.AddGeneratedCardToCombat(bitters, PileType.Hand, Owner);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}
