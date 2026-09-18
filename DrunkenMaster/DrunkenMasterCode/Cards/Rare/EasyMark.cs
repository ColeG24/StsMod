using DrunkenMaster.DrunkenMasterCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Rare;

/// <summary>Rare Power, 1 Energy. Enemies with Confusion take 25% more damage from Attacks. Upgraded: 50% (2026-09-18; was 2 Energy, 50%, upgrade cut the cost).</summary>
public class EasyMark() : DrunkenMasterCard(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<EasyMarkPower>(25)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<EasyMarkPower>(), HoverTipFactory.FromPower<ConfusionPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        NPowerUpVfx.CreateNormal(Owner.Creature);
        await PowerCmd.Apply<EasyMarkPower>(choiceContext, Owner.Creature, DynamicVars[nameof(EasyMarkPower)].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars[nameof(EasyMarkPower)].UpgradeValueBy(25m);
}
