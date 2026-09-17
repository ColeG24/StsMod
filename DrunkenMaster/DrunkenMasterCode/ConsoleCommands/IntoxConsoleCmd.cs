using DrunkenMaster.DrunkenMasterCode.Resources;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace DrunkenMaster.DrunkenMasterCode.ConsoleCommands;

/// <summary>
/// Dev console: `intox 3` gains 3, `intox -2` loses 2, `intox set 8` goes straight to 8. Testing aid (2026-09-17).
/// Goes through the same paths as cards (GainAsync, or Lose + FlushBandPowers + NotifyLost), so band powers, band-up
/// listeners (Tolerance) and loss listeners (Walk It Off) all react. The game finds this class by reflection.
/// </summary>
public class IntoxConsoleCmd : AbstractConsoleCmd
{
    public override string CmdName => "intox";
    public override string Args => "[set] <amount:int>";
    public override string Description => "Gain (or with a negative amount, lose) Intoxication. 'intox set <n>' sets it outright.";
    public override bool IsNetworked => true;

    public override CmdResult Process(Player? issuingPlayer, string[] args)
    {
        if (issuingPlayer?.PlayerCombatState == null) return new CmdResult(success: false, "This command only works in combat.");
        var resource = IntoxicationResource.Get(issuingPlayer);
        if (resource == null) return new CmdResult(success: false, "This player has no Intoxication.");

        bool set = args.Length >= 1 && args[0].Equals("set", StringComparison.OrdinalIgnoreCase);
        string? raw = set ? args.ElementAtOrDefault(1) : args.FirstOrDefault();
        if (!int.TryParse(raw, out int amount)) return new CmdResult(success: false, "Usage: intox " + Args);

        int target = Math.Clamp(set ? amount : resource.Amount + amount, 0, IntoxicationResource.Max);
        var task = ChangeTo(issuingPlayer, resource, target);
        return new CmdResult(task, success: true, $"Intoxication {resource.Amount} -> {target}");
    }

    private static async Task ChangeTo(Player player, IntoxicationResource resource, int target)
    {
        var context = new BlockingPlayerChoiceContext();
        int current = resource.Amount;
        if (target > current) await IntoxicationResource.GainAsync(context, player, target - current);
        else if (target < current)
        {
            IntoxicationResource.Lose(player, current - target);
            await resource.FlushBandPowers(context);
            await IntoxicationResource.NotifyLost(context, player, current - target);
        }
    }
}
