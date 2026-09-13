using DrunkenMaster.DrunkenMasterCode.Powers;
using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Rare;

/// <summary>
/// Rare Power, 2 Energy. While Drunk, your cards' random costs can never go up (same or cheaper).
/// Upgraded: also gain 4 Intoxication when played (once, not per turn).
/// </summary>
public class Tolerance() : DrunkenMasterCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    public const string IntoxicationKey = "Intoxication";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(IntoxicationKey, 0)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<TolerancePower>(), IntoxicationResource.Tip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        NPowerUpVfx.CreateNormal(Owner.Creature);
        await PowerCmd.Apply<TolerancePower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
        int intoxication = DynamicVars[IntoxicationKey].IntValue;
        if (intoxication > 0) await IntoxicationResource.GainAsync(choiceContext, Owner, intoxication);
    }

    protected override void OnUpgrade() => DynamicVars[IntoxicationKey].UpgradeValueBy(4m);
}
