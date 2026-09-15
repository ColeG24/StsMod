using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace DrunkenMaster.DrunkenMasterCode.Relics;

/// <summary>Common. At the start of combat, if at least half of your potion slots are full, gain 2 Strength.</summary>
public class Bandolier : DrunkenMasterRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(2)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    public static bool IsHalfFull(MegaCrit.Sts2.Core.Entities.Players.Player player) =>
        player.MaxPotionCount > 0 && player.Potions.Count() * 2 >= player.MaxPotionCount;

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is not CombatRoom || !IsHalfFull(Owner)) return;
        Flash();
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars[nameof(StrengthPower)].BaseValue, Owner.Creature, null);
    }
}
