using DrunkenMaster.DrunkenMasterCode.Powers;
using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Rare;

/// <summary>
/// Rare Power, 2 Energy. If you end your turn Drunk, Block is not removed at the start of your next turn.
/// Upgraded: costs 1. 2026-09-19: the class's defensive Rare Power (Barricade with a band gate), built as "Dead Drunk" and
/// folded into the Iron Liver slot the same day, replacing "whenever you drink a potion, gain 2 (3) Vigor" (2026-09-16;
/// before that an Uncommon giving 3 (5) Block per drink). Keeps the Iron Liver art.
/// </summary>
public class IronLiver() : DrunkenMasterCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<IronLiverPower>(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<IronLiverPower>(),
        IntoxicationResource.Tip,
        IntoxicationResource.BandTip(IntoxicationResource.Band.Drunk),
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        NPowerUpVfx.CreateNormal(Owner.Creature);
        await PowerCmd.Apply<IronLiverPower>(choiceContext, Owner.Creature, DynamicVars[nameof(IronLiverPower)].BaseValue, Owner.Creature, this);
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
