using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace DrunkenMaster.DrunkenMasterCode.Relics;

/// <summary>
/// Uncommon. Start each combat with 3 Intoxication. Lands on turn 1 like Tavern Rag's Ingredient, after the turn-start
/// decay has run (from 0 that decay is a no-op), so the dial reads 3 when the first hand is in your hand.
/// </summary>
public class HipFlask : DrunkenMasterRelic
{
    public const string IntoxicationKey = "Intoxication";

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(IntoxicationKey, 3)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [IntoxicationResource.Tip];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || (player.PlayerCombatState?.TurnNumber ?? 1) > 1) return;
        Flash();
        await IntoxicationResource.GainAsync(choiceContext, Owner, DynamicVars[IntoxicationKey].IntValue);
    }
}
