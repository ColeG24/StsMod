using BaseLib.Abstracts;
using BaseLib.Patches.UI;
using DrunkenMaster.DrunkenMasterCode.Resources;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace DrunkenMaster.DrunkenMasterCode.Ui;

/// <summary>
/// Groups the Intoxication dial and the Brew pot and parks them above the character's head:
/// dial on the left, pot to its right. Follows the creature node every frame.
/// </summary>
public partial class NDrunkenMasterHud : Control
{
    private const float Spacing = 10f;
    private const float AboveHead = 28f;

    private readonly Player _player;
    private readonly NIntoxicationDial _dial;
    private readonly NBrewDisplay _brew;

    public static void Register()
    {
        ExtraCombatUi.RegisterCombatUiElement((ui, player, _) =>
        {
            if (player.Character is not Character.DrunkenMaster) return null;
            var state = player.PlayerCombatState;
            if (state == null) return null;
            var resource = CustomResources<IntoxicationResource>.Get(state);
            var hud = new NDrunkenMasterHud(player, resource);
            ui.AddChild(hud);
            return hud;
        });
    }

    public NDrunkenMasterHud(Player player, IntoxicationResource resource)
    {
        _player = player;
        Name = "DrunkenMasterHud";
        SetMouseFilter(MouseFilterEnum.Ignore);

        _dial = new NIntoxicationDial(player, resource);
        _brew = new NBrewDisplay(player);

        _dial.SetAnchorsPreset(LayoutPreset.TopLeft);
        Layout();

        AddChild(_dial);
        AddChild(_brew);
        Visible = false;
    }

    /// <summary>Dial vertically centred against the pot panel. Re-run each frame: the pot can change width.</summary>
    private void Layout()
    {
        float height = Mathf.Max(_dial.Size.Y, _brew.Size.Y);
        _dial.Position = new Vector2(0, (height - _dial.Size.Y) / 2f);
        _brew.Position = new Vector2(_dial.Size.X + Spacing, (height - _brew.Size.Y) / 2f);
        Size = new Vector2(_dial.Size.X + Spacing + _brew.Size.X, height);
    }

    public override void _Process(double delta)
    {
        Layout();
        var creatureNode = NCombatRoom.Instance?.GetCreatureNode(_player.Creature);
        if (creatureNode == null || !GodotObject.IsInstanceValid(creatureNode))
        {
            Visible = false;
            return;
        }
        Visible = true;
        var top = creatureNode.GetTopOfHitbox();
        GlobalPosition = new Vector2(top.X - Size.X / 2f, top.Y - Size.Y - AboveHead);
    }
}
