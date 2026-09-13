using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace DrunkenMaster.DrunkenMasterCode.Cards.Basic;

/// <summary>Gain 5 Block. Upgraded: 8.</summary>
public class DefendDrunkenMaster() : DrunkenMasterCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    // Reuse the base-game Ironclad art until the character has its own.
    public override string PortraitPath => ImageHelper.GetImagePath("atlases/card_atlas.sprites/ironclad/defend_ironclad.tres");
    public override string BetaPortraitPath => ImageHelper.GetImagePath("atlases/card_atlas.sprites/ironclad/beta/defend_ironclad.tres");
    public override string CustomPortraitPath => ImageHelper.GetImagePath("packed/card_portraits/ironclad/defend_ironclad.png");

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}
