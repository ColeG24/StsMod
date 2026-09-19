using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Powers;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Uncommon;

/// <summary>
/// Uncommon Power, 2 cost. Your Brew holds 1 more Ingredient. Gain 2 Strength and 2 Dexterity. Upgraded: 3 and 3.
/// 2026-09-19: Bulk Up's shape and price (Defect, 2 cost, -1 orb slot, +2 (3) Strength and Dexterity) with the
/// capacity change pointing the other way. Was 1 cost, capacity only, upgrade to 0: offered 9 times, never taken.
/// The bigger pot is the ambivalent rider (slower drinks, bigger Everclear / Stir Crazy / Chaser potions); the
/// stats are the reason to pick it.
/// </summary>
public class Stockpot() : DrunkenMasterCard(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StockpotPower>(1),
        new PowerVar<StrengthPower>(2),
        new PowerVar<DexterityPower>(2)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StockpotPower>(),
        HoverTipFactory.Static(DrunkenMasterTips.Brew),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>()
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        NPowerUpVfx.CreateNormal(Owner.Creature);
        await PowerCmd.Apply<StockpotPower>(choiceContext, Owner.Creature, DynamicVars[nameof(StockpotPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, DynamicVars.Strength.BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, DynamicVars.Dexterity.BaseValue, Owner.Creature, this);
        await BrewSystem.RecheckSeal(Owner);
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Strength.UpgradeValueBy(1m);
        DynamicVars.Dexterity.UpgradeValueBy(1m);
    }
}
