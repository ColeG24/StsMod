using DrunkenMaster.DrunkenMasterCode.Powers;
using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Uncommon;

/// <summary>Uncommon Power, 1 Energy (2026-09-18; was Rare at 2). Whenever you Blackout (end a turn at 12 Intoxication), gain 3 Strength. Upgraded: 4. (2026-09-13: was +1/2 per band raised.)</summary>
public class DutchCourage() : DrunkenMasterCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DutchCouragePower>(3)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<DutchCouragePower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        IntoxicationResource.Tip,
        IntoxicationResource.BandTip(IntoxicationResource.Band.Blackout),
        HoverTipFactory.FromPower<HungoverPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        NPowerUpVfx.CreateNormal(Owner.Creature);
        await PowerCmd.Apply<DutchCouragePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(DutchCouragePower)].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars[nameof(DutchCouragePower)].UpgradeValueBy(1m);
}
