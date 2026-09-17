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

namespace DrunkenMaster.DrunkenMasterCode.Cards.Ancient;

/// <summary>
/// Ancient Attack, 1 Energy. Deal 20 damage. Add 2 Ethanol into your hand. Upgraded: 30, and the Ethanol are Upgraded.
/// Hard Liquor's Archaic Tooth transcendence (2026-09-16), built on Bash -> Break: 1 Energy cheaper, damage x ~2, the rider
/// doubled, the upgrade step scaled with it (+10). Hard Liquor names it through BaseLib's ITranscendenceCard, which also
/// puts it in ArchaicTooth.TranscendenceCards, so Dusty Tome keeps handing out Top Shelf and never this.
/// The name is a placeholder pick; the art is a crop of Hard Liquor's.
/// </summary>
public class CaskStrength() : DrunkenMasterCard(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(20, ValueProp.Move),
        new DynamicVar(EthanolsKey, 2)
    ];

    public const string EthanolsKey = "Ethanols";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<Ethanol>(IsUpgraded),
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
        for (int i = 0; i < DynamicVars[EthanolsKey].IntValue; i++)
        {
            var ethanol = BrewSystem.CreateIngredient(Owner, ModelDb.Card<Ethanol>(), upgraded: IsUpgraded);
            await CardPileCmd.AddGeneratedCardToCombat(ethanol, PileType.Hand, Owner);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(10m);
}
