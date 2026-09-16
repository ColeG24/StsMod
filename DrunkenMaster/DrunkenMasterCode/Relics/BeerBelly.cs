using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Relics;

/// <summary>
/// Rare (2026-09-16, the third Rare that brings the pool to base parity). Whenever you drink a potion, gain 3 Block.
/// Every potion counts (Concoctions, Dregs, the character potions, shared ones), uncapped: a flat per-drink payoff, and
/// drinks are bounded by the Ingredients spent to brew them. Unpowered like Anchor, so Drunk's -1 Dexterity does not
/// touch it. Same <see cref="AfterPotionUsed"/> hook as Iron Liver / Shot Glass.
/// </summary>
public class BeerBelly : DrunkenMasterRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(3, ValueProp.Unpowered)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block)];

    public override async Task AfterPotionUsed(PotionModel potion, Creature? target)
    {
        if (potion.Owner != Owner || Owner.Creature.IsDead || Owner.Creature.CombatState == null) return;
        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, null);
    }
}
