using DrunkenMaster.DrunkenMasterCode.Resources;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace DrunkenMaster.DrunkenMasterCode.Potions;

/// <summary>
/// Common character potion (2026-09-14). Gain 6 Intoxication: Sober straight to Tipsy, or Tipsy over the Drunk line.
/// Goes through GainAsync so band-raised listeners (Dutch Courage) fire and the Tipsy Str/Dex lands.
/// </summary>
public class Firewater : DrunkenMasterPotion
{
    public const string IntoxicationKey = "Intoxication";

    public override PotionRarity Rarity => PotionRarity.Common;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(IntoxicationKey, 6)];

    public override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        NCombatRoom.Instance?.PlaySplashVfx(Owner.Creature, new Color("f08a3c"));
        await IntoxicationResource.GainAsync(choiceContext, Owner, DynamicVars[IntoxicationKey].IntValue);
    }
}
