using DrunkenMaster.DrunkenMasterCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Rare;

/// <summary>
/// Rare Power, 1 Energy (2026-09-16; was an Uncommon that gave 3 (5) Block per drink, redundant next to Barback and
/// Walk It Off). Whenever you drink a potion, gain 2 Vigor. Upgraded: 3. Vigor is the game's own next-Attack buff, so a
/// drink sets up the hit rather than generating any resource (no Shot Glass loop).
/// </summary>
public class IronLiver() : DrunkenMasterCard(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<IronLiverPower>(2)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IronLiverPower>(), HoverTipFactory.FromPower<VigorPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        NPowerUpVfx.CreateNormal(Owner.Creature);
        await PowerCmd.Apply<IronLiverPower>(choiceContext, Owner.Creature, DynamicVars[nameof(IronLiverPower)].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars[nameof(IronLiverPower)].UpgradeValueBy(1m);
}
