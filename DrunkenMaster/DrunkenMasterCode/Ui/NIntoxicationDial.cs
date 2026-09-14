using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.HoverTips;
using BaseLib.Abstracts;
using BaseLib.BaseLibScenes;
using DrunkenMaster.DrunkenMasterCode.Resources;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;

namespace DrunkenMaster.DrunkenMasterCode.Ui;

/// <summary>
/// The Intoxication dial: a 3/4-circle gauge with the four bands painted as arcs, a needle at the
/// current amount, and the number in the middle (label provided by the BaseLib base class).
/// </summary>
public partial class NIntoxicationDial : NAdditionalResourceDisplay
{
    private readonly NDialGauge _gauge;
    private readonly Label _bandLabel;
    private readonly Label _forecastLabel;
    private readonly IntoxicationResource? _intoxication;
    private readonly Player _owner;
    private (int projected, int delta) _forecast = (int.MinValue, 0);

    private static readonly Color Up = new("8fd67a");
    private static readonly Color Down = new("e07a6a");
    private static readonly Color Flat = new("b9b2a3");

    public NIntoxicationDial(Player player, CustomResource resource) : base(player, resource)
    {
        Size = new Vector2(96, 96);
        _gauge = new NDialGauge { Name = "Gauge" };
        _gauge.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _gauge.SetMouseFilter(MouseFilterEnum.Ignore);
        _visuals.AddChild(_gauge);
        _gauge.SetAmount(resource.Amount);

        // Band name in the open bottom of the 3/4 ring, painted in the band colour.
        _bandLabel = new Label
        {
            Name = "Band",
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore,
            Position = new Vector2(0, Size.Y - 20),
            Size = new Vector2(Size.X, 18)
        };
        _bandLabel.AddThemeFontSizeOverride("font_size", 13);
        _bandLabel.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 0.9f));
        _bandLabel.AddThemeConstantOverride("outline_size", 5);
        _visuals.AddChild(_bandLabel);

        // Forecast for next turn ("-1 / turn", "+1 / turn"), tucked inside the ring above the number.
        _forecastLabel = new Label
        {
            Name = "Forecast",
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore,
            Position = new Vector2(0, 15),
            Size = new Vector2(Size.X, 14)
        };
        _forecastLabel.AddThemeFontSizeOverride("font_size", 11);
        _forecastLabel.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 0.9f));
        _forecastLabel.AddThemeConstantOverride("outline_size", 4);
        _visuals.AddChild(_forecastLabel);

        _owner = player;
        _intoxication = resource as IntoxicationResource;
        SetBand(IntoxicationResource.BandFor(resource.Amount));
        RefreshForecast();
        if (_intoxication != null) _intoxication.BandChanged += OnBandChanged;

        DisplayAmountChanged += (_, newAmount) => _gauge.SetAmount(newAmount);
        // The base class paints the label red at 0; Sober is a normal state, keep it cream.
        AfterLabelAmountChanged += _ => _label.AddThemeColorOverride("font_color", new Color("FFF6E2"));
        SetAnchorsPreset(LayoutPreset.TopLeft);
        PivotOffset = Size / 2f;
        Visible = true;
    }

    /// <summary>
    /// The BaseLib base shows a single tip built from the resource's title/description. The dial instead shows the
    /// short Intoxication tip followed by one tip per band (2026-09-14), so we blank the base tip and hang our own
    /// handlers on the same signals.
    /// </summary>
    public override void _Ready()
    {
        base._Ready();
        _hoverTip = null;
        MouseEntered += ShowTips;
        MouseExited += HideTips;
    }

    private void ShowTips()
    {
        var set = NHoverTipSet.CreateAndShow(this, IntoxicationResource.AllTips, HoverTipAlignment.None);
        if (set == null) return;
        // The set grows its text column as tips are added, so the stack height is known here. Anchor the bottom of
        // the stack just above the dial and keep the top on screen; the base class's fixed -300 offset cut off the
        // five-tip stack (2026-09-14).
        float height = set.GetNodeOrNull<Control>("textHoverTipContainer")?.Size.Y ?? 300f;
        float y = Mathf.Max(12f, GlobalPosition.Y - height - 8f);
        set.GlobalPosition = new Vector2(GlobalPosition.X - 34f, y);
    }

    private void HideTips() => NHoverTipSet.Remove(this);

    public override void _ExitTree()
    {
        if (_intoxication != null) _intoxication.BandChanged -= OnBandChanged;
        base._ExitTree();
    }

    /// <summary>Powers come and go without any event we can hook, so poll the cheap projection every frame.</summary>
    public override void _Process(double delta)
    {
        base._Process(delta);
        RefreshForecast();
    }

    private void RefreshForecast()
    {
        var forecast = IntoxicationResource.ProjectNextTurn(_owner);
        if (forecast == _forecast) return;
        _forecast = forecast;
        _gauge.SetProjection(forecast.projected);
        int d = forecast.delta;
        _forecastLabel.Text = d == 0 ? "0 / turn" : $"{(d > 0 ? "+" : "-")}{Math.Abs(d)} / turn";
        _forecastLabel.AddThemeColorOverride("font_color", d > 0 ? Up : d < 0 ? Down : Flat);
    }

    private void SetBand(IntoxicationResource.Band band)
    {
        _bandLabel.Text = IntoxicationResource.BandName(band).ToUpperInvariant();
        _bandLabel.AddThemeColorOverride("font_color", IntoxicationResource.BandColor(band));
    }

    /// <summary>Snap the label to the new band and pop the whole dial so the eye lands on it.</summary>
    private void OnBandChanged(IntoxicationResource.Band from, IntoxicationResource.Band to)
    {
        SetBand(to);
        if (!IsInsideTree()) return;
        float pop = to > from ? 1.45f : 1.2f;
        var tween = CreateTween();
        tween.TweenProperty(this, "scale", Vector2.One * pop, 0.12).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quad);
        tween.TweenProperty(this, "scale", Vector2.One, 0.45).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);
    }
}

/// <summary>Pure-drawn gauge. No scene, no textures.</summary>
public partial class NDialGauge : Control
{
    private const float StartAngle = 0.75f * Mathf.Pi;   // 135°, lower-left
    private const float Sweep = 1.5f * Mathf.Pi;         // 270° to lower-right

    private int _amount;
    private float _shownAmount;
    private int _projected = -1;

    public void SetAmount(int amount)
    {
        _amount = Mathf.Clamp(amount, 0, IntoxicationResource.Max);
    }

    /// <summary>Where the needle will sit after the next turn start. -1 hides the ghost.</summary>
    public void SetProjection(int projected)
    {
        _projected = projected < 0 ? -1 : Mathf.Clamp(projected, 0, IntoxicationResource.Max);
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        float target = _amount;
        if (Mathf.Abs(_shownAmount - target) > 0.01f)
        {
            _shownAmount = Mathf.Lerp(_shownAmount, target, (float)(delta * 10));
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        var center = Size / 2f;
        float radius = Mathf.Min(Size.X, Size.Y) / 2f - 6f;
        const float width = 9f;
        int max = IntoxicationResource.Max;

        // Backing ring.
        DrawArc(center, radius, StartAngle, StartAngle + Sweep, 48, new Color(0, 0, 0, 0.55f), width + 4, true);

        // Band segments, dimmed.
        DrawBand(center, radius, width, 0, IntoxicationResource.TipsyMin, IntoxicationResource.Band.Sober, 0.35f);
        DrawBand(center, radius, width, IntoxicationResource.TipsyMin, IntoxicationResource.DrunkMin, IntoxicationResource.Band.Tipsy, 0.35f);
        DrawBand(center, radius, width, IntoxicationResource.DrunkMin, IntoxicationResource.BlackoutAt, IntoxicationResource.Band.Drunk, 0.35f);

        // Filled portion, full brightness, in the colour of the current band.
        if (_shownAmount > 0.01f)
        {
            float end = StartAngle + Sweep * (_shownAmount / max);
            var color = IntoxicationResource.BandColor(IntoxicationResource.BandFor(_amount));
            DrawArc(center, radius, StartAngle, end, 48, color, width, true);
        }

        // Blackout pip at the end of the dial.
        var pipAngle = StartAngle + Sweep;
        var pip = center + Vector2.FromAngle(pipAngle) * radius;
        DrawCircle(pip, width * 0.7f, IntoxicationResource.BandColor(IntoxicationResource.Band.Blackout) * (_amount >= max ? 1f : 0.45f));

        // Band thresholds: a tick across the ring and the number just outside it, so the tooltip can stay wordless
        // about numbers (2026-09-14).
        DrawThreshold(center, radius, width, IntoxicationResource.TipsyMin, IntoxicationResource.Band.Tipsy);
        DrawThreshold(center, radius, width, IntoxicationResource.DrunkMin, IntoxicationResource.Band.Drunk);
        DrawThreshold(center, radius, width, IntoxicationResource.BlackoutAt, IntoxicationResource.Band.Blackout);

        // Ghost of next turn: a thin arc from now to then (green up, red down) and a faint needle.
        if (_projected >= 0 && _projected != _amount)
        {
            float aNow = StartAngle + Sweep * ((float)_amount / max);
            float aNext = StartAngle + Sweep * ((float)_projected / max);
            bool up = _projected > _amount;
            var c = up ? new Color("8fd67a") : new Color("e07a6a");
            DrawArc(center, radius + width * 0.5f + 3f, Mathf.Min(aNow, aNext), Mathf.Max(aNow, aNext), 16, new Color(c.R, c.G, c.B, 0.9f), 3f, true);
            var ghostTip = center + Vector2.FromAngle(aNext) * (radius - width);
            DrawLine(center, ghostTip, new Color(c.R, c.G, c.B, 0.55f), 2f, true);
        }

        // Needle.
        float needleAngle = StartAngle + Sweep * (_shownAmount / max);
        var tip = center + Vector2.FromAngle(needleAngle) * (radius - width);
        DrawLine(center, tip, new Color("FFF6E2"), 3f, true);
        DrawCircle(center, 4f, new Color("FFF6E2"));
    }

    private void DrawThreshold(Vector2 center, float radius, float width, int at, IntoxicationResource.Band band)
    {
        float angle = StartAngle + Sweep * ((float)at / IntoxicationResource.Max);
        var dir = Vector2.FromAngle(angle);
        var c = IntoxicationResource.BandColor(band);
        DrawLine(center + dir * (radius - width * 0.5f - 1f), center + dir * (radius + width * 0.5f + 1f), new Color(0, 0, 0, 0.8f), 2f, true);
        var font = ThemeDB.FallbackFont;
        const int fontSize = 10;
        string text = at.ToString();
        var textSize = font.GetStringSize(text, HorizontalAlignment.Left, -1, fontSize);
        // Baseline-positioned: shift so the glyph box is centred on the point just outside the ring.
        var pos = center + dir * (radius + width * 0.5f + 8f) + new Vector2(-textSize.X / 2f, textSize.Y / 2f - 2f);
        DrawStringOutline(font, pos, text, HorizontalAlignment.Left, -1, fontSize, 4, new Color(0, 0, 0, 0.9f));
        DrawString(font, pos, text, HorizontalAlignment.Left, -1, fontSize, c);
    }

    private void DrawBand(Vector2 center, float radius, float width, int from, int to, IntoxicationResource.Band band, float alpha)
    {
        int max = IntoxicationResource.Max;
        float a0 = StartAngle + Sweep * ((float)from / max);
        float a1 = StartAngle + Sweep * ((float)to / max);
        var c = IntoxicationResource.BandColor(band);
        DrawArc(center, radius, a0, a1, 24, new Color(c.R, c.G, c.B, alpha), width, true);
    }
}
