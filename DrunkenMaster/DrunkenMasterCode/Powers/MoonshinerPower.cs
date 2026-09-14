using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Potions;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Powers;

/// <summary>
/// Whenever your Brew seals into a Concoction, deal Amount damage to ALL enemies for each Ingredient in it.
///
/// Damage since 2026-09-14 (was +2 Energy per seal, which overlapped Bottomless Cup and fed an infinite). Scaled per
/// Ingredient so the payout is bounded by Ingredients played, not by how often the pot seals. Unpowered like
/// Juggernaut: Strength does not touch it.
/// </summary>
public class MoonshinerPower : DrunkenMasterPower, BrewSystem.IBrewSealedListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(DrunkenMasterTips.Brew)];

    public async Task OnBrewSealed(Player owner, Concoction potion)
    {
        if (owner != Owner.Player) return;
        var combatState = Owner.CombatState;
        if (combatState == null) return;
        int damage = Amount * Concoction.Ingredients[potion].Count;
        if (damage <= 0) return;
        Flash();
        foreach (var enemy in combatState.HittableEnemies.ToList())
        {
            if (!enemy.IsAlive) continue;
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), enemy, damage, ValueProp.Unpowered, Owner);
        }
    }
}
