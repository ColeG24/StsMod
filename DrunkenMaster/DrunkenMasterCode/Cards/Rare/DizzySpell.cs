using DrunkenMaster.DrunkenMasterCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Rare;

/// <summary>
/// Rare Skill, 1 Energy (2026-09-16). Double the Confusion on an enemy. Upgraded: Retain (cost stays 1).
/// Bounded Confusion enabler: the doubled stacks still expire at end of turn. Goes through PowerCmd.Apply so Absinthe
/// Spoon adds its +1 and The Spins deals its damage on the doubled amount. Does nothing on an enemy with no Confusion.
/// </summary>
public class DizzySpell() : DrunkenMasterCard(1, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ConfusionPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var target = cardPlay.Target;
        decimal confusion = target.Powers.OfType<ConfusionPower>().FirstOrDefault()?.Amount ?? 0;
        if (confusion <= 0 || !target.IsAlive) return;
        await PowerCmd.Apply<ConfusionPower>(choiceContext, target, confusion, Owner.Creature, this);
    }

    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
