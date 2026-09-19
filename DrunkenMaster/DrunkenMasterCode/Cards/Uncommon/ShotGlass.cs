using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Powers;
using DrunkenMaster.DrunkenMasterCode.Resources;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Uncommon;

/// <summary>
/// Uncommon Power, 1 cost. Your Brew holds 1 fewer Ingredient. Ingredients cost 1 more Intoxication to play.
/// Upgraded: costs 0. (2026-09-19 rework. Until then: +1 (2) Intoxication whenever you drink a Concoction.
/// Until 2026-09-14: 2 cost, +2 (3) Strength and Dexterity.)
/// </summary>
public class ShotGlass() : DrunkenMasterCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<ShotGlassPower>(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<ShotGlassPower>(),
        HoverTipFactory.Static(DrunkenMasterTips.Brew),
        HoverTipFactory.Static(DrunkenMasterTips.Ingredient),
        IntoxicationResource.Tip
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        NPowerUpVfx.CreateNormal(Owner.Creature);
        await PowerCmd.Apply<ShotGlassPower>(choiceContext, Owner.Creature, DynamicVars[nameof(ShotGlassPower)].BaseValue, Owner.Creature, this);
        ShotGlassPower.RefreshIngredientCosts(Owner);
        await BrewSystem.RecheckSeal(Owner);
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
