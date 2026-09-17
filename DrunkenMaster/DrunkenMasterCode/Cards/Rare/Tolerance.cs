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
/// Rare Power, 2 Energy. The first time you are Drunk each turn, gain 2 Energy. Upgraded: 3.
/// 2026-09-17 rework; was "while Drunk your cards' random costs can never go up" with 4 Intoxication on the upgrade.
/// Played while already Drunk it pays out on the spot: that is this turn's first time.
/// </summary>
public class Tolerance() : DrunkenMasterCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<TolerancePower>(2)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<TolerancePower>(), IntoxicationResource.Tip, IntoxicationResource.BandTip(IntoxicationResource.Band.Drunk)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        NPowerUpVfx.CreateNormal(Owner.Creature);
        var power = await PowerCmd.Apply<TolerancePower>(choiceContext, Owner.Creature, DynamicVars[nameof(TolerancePower)].BaseValue, Owner.Creature, this);
        if (power != null) await power.TryPayOut();
    }

    protected override void OnUpgrade() => DynamicVars[nameof(TolerancePower)].UpgradeValueBy(1m);
}
