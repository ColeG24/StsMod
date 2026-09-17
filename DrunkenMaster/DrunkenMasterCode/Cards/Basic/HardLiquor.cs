using BaseLib.Abstracts;
using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Cards.Ancient;
using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Basic;

/// <summary>
/// Starter (the class's Bash). 2 Energy. Deal 12 damage. Add an Ethanol into your hand. Upgraded: 16, and the Ethanol is Upgraded.
/// Was an Uncommon that granted Intoxication directly; since 2026-09-12 the drunkenness comes from brewing the Ethanol.
/// Archaic Tooth transforms it into Cask Strength (2026-09-16).
/// </summary>
public class HardLiquor() : DrunkenMasterCard(2, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy), ITranscendenceCard
{
    public CardModel GetTranscendenceTransformedCard() => ModelDb.Card<CaskStrength>();

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12, ValueProp.Move)];
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
        var ethanol = BrewSystem.CreateIngredient(Owner, ModelDb.Card<Ethanol>(), upgraded: IsUpgraded);
        await CardPileCmd.AddGeneratedCardToCombat(ethanol, PileType.Hand, Owner);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}
