using DrunkenMaster.DrunkenMasterCode.Powers;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace DrunkenMaster.DrunkenMasterCode.Potions;

/// <summary>Uncommon character potion (2026-09-14). Throw it at an enemy: apply 7 Confusion.</summary>
public class BathtubGin : DrunkenMasterPotion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.AnyEnemy;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<ConfusionPower>(7)];

    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ConfusionPower>()];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        AssertValidForTargetedPotion(target);
        NCombatRoom.Instance?.PlaySplashVfx(target, new Color("c9b7ff"));
        if (!target.IsAlive) return;
        await PowerCmd.Apply<ConfusionPower>(choiceContext, target, DynamicVars[nameof(ConfusionPower)].BaseValue, Owner.Creature, null);
    }
}
