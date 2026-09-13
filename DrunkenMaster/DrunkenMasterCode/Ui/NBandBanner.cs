using DrunkenMaster.DrunkenMasterCode.Resources;
using Godot;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using Band = DrunkenMaster.DrunkenMasterCode.Resources.IntoxicationResource.Band;

namespace DrunkenMaster.DrunkenMasterCode.Ui;

/// <summary>
/// Full-screen announcement when Intoxication crosses a band: a colour flash, the band name in big
/// letters, a one-line reminder of what the band does, a screen shake and a slosh. Modelled on the
/// game's NPlayerTurnBanner (fade + drift in, hold, fade out, free itself). Going up is loud and
/// gets louder per band; sobering up is a quieter, smaller version of the same thing.
/// </summary>
public partial class NBandBanner : Control
{
    private const float CenterOffsetY = -180f;   // above the middle so it never covers the hand

    private static readonly Color Cream = new("FFF6E2");

    private readonly Band _from;
    private readonly Band _to;
    private ColorRect _flash = null!;
    private Label _title = null!;
    private Label _subtitle = null!;

    public static void Show(Player player, Band from, Band to)
    {
        var room = NCombatRoom.Instance;
        if (room == null || !GodotObject.IsInstanceValid(room)) return;
        if (player.Creature == null || player.Creature.IsDead) return;
        room.AddChildSafely(new NBandBanner(from, to));
    }

    private NBandBanner(Band from, Band to)
    {
        _from = from;
        _to = to;
        Name = "BandBanner";
        MouseFilter = MouseFilterEnum.Ignore;
    }

    private bool GoingUp => _to > _from;

    private static string Subtitle(Band band) => band switch
    {
        Band.Sober => "Brew while you can",
        Band.Tipsy => "+2 damage, +2 Block",
        Band.Drunk => "Card costs are random",
        _ => $"End of turn: play {IntoxicationResource.BlackoutCardsPlayed} from draw pile, Exhaust hand",
    };

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        var color = IntoxicationResource.BandColor(_to);
        bool loud = GoingUp;
        int titleSize = _to switch { Band.Blackout => 110, Band.Drunk => 92, _ => 80 };
        if (!loud) titleSize = 64;

        _flash = new ColorRect { Name = "Flash", Color = color, MouseFilter = MouseFilterEnum.Ignore };
        _flash.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _flash.Modulate = new Color(1, 1, 1, 0);
        AddChild(_flash);

        _title = MakeLabel("Title", IntoxicationResource.BandName(_to).ToUpperInvariant(), titleSize, color, CenterOffsetY);
        _subtitle = MakeLabel("Subtitle", Subtitle(_to), 26, Cream, CenterOffsetY + titleSize * 0.75f);
        AddChild(_title);
        AddChild(_subtitle);

        Modulate = Colors.Transparent;
        Punch();
        Animate();
    }

    private Label MakeLabel(string name, string text, int fontSize, Color color, float offsetY)
    {
        var label = new Label
        {
            Name = name,
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore
        };
        label.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        label.OffsetTop = offsetY;
        label.OffsetBottom = offsetY;
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.AddThemeColorOverride("font_color", color);
        label.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 0.95f));
        label.AddThemeConstantOverride("outline_size", Mathf.Max(6, fontSize / 6));
        return label;
    }

    /// <summary>Shake and sound scale with how hard you just hit the bottle.</summary>
    private void Punch()
    {
        if (!GoingUp) return;
        var game = NGame.Instance;
        switch (_to)
        {
            case Band.Tipsy:
                game?.ScreenShake(ShakeStrength.Weak, ShakeDuration.Short);
                NDebugAudioManager.Instance?.Play("potion_slosh_1.mp3", 1f, PitchVariance.Small);
                break;
            case Band.Drunk:
                game?.ScreenShake(ShakeStrength.Medium, ShakeDuration.Short);
                NDebugAudioManager.Instance?.Play("potion_slosh_2.mp3", 1f, PitchVariance.Small);
                break;
            case Band.Blackout:
                game?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Normal);
                NDebugAudioManager.Instance?.Play("potion_slosh_3.mp3", 1f, PitchVariance.Small);
                NDebugAudioManager.Instance?.Play("heavy_attack.mp3", 0.8f);
                break;
        }
    }

    private void Animate()
    {
        bool loud = GoingUp;
        float flashAlpha = _to switch { Band.Blackout => 0.45f, Band.Drunk => 0.28f, Band.Tipsy => 0.18f, _ => 0f };
        float hold = _to == Band.Blackout ? 1.1f : loud ? 0.7f : 0.4f;
        float drift = loud ? -40f : -20f;

        var size = GetViewportRect().Size;
        foreach (var label in new[] { _title, _subtitle })
        {
            label.PivotOffset = size / 2f + new Vector2(0, label.OffsetTop);
            label.Scale = Vector2.One * (loud ? 1.35f : 1.1f);
        }

        var tween = CreateTween();
        tween.SetParallel();
        tween.TweenProperty(this, "modulate:a", 1f, 0.35).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
        tween.TweenProperty(_title, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
        tween.TweenProperty(_subtitle, "scale", Vector2.One, 0.5).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
        tween.TweenProperty(_title, "position", _title.Position + new Vector2(0, drift), 1.2).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
        tween.TweenProperty(_subtitle, "position", _subtitle.Position + new Vector2(0, -drift * 0.5f), 1.2).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
        if (flashAlpha > 0)
        {
            tween.TweenProperty(_flash, "modulate:a", flashAlpha, 0.08);
            tween.Chain().TweenProperty(_flash, "modulate:a", 0f, 0.6).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
        }

        var outTween = CreateTween();
        outTween.TweenInterval(0.35 + hold);
        outTween.TweenProperty(this, "modulate:a", 0f, 0.4).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
        outTween.TweenCallback(Callable.From(() => this.QueueFreeSafely()));
    }
}
