using DrunkenMaster.DrunkenMasterCode.Powers;
using DrunkenMaster.DrunkenMasterCode.Tips;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Rare;

/// <summary>Rare Power, 2 cost. Whenever your Brew seals into a Concoction, deal 3 damage to ALL enemies for each Ingredient in it. Upgraded: 4. (Was +2 Energy per seal, upgrade cost 1, until 2026-09-14.)</summary>
public class Moonshiner() : DrunkenMasterCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MoonshinerPower>(3)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<MoonshinerPower>(),
        HoverTipFactory.Static(DrunkenMasterTips.Brew)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        NPowerUpVfx.CreateNormal(Owner.Creature);
        await PowerCmd.Apply<MoonshinerPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(MoonshinerPower)].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars[nameof(MoonshinerPower)].UpgradeValueBy(1m);
}
