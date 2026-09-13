using DrunkenMaster.DrunkenMasterCode.Brew;
using DrunkenMaster.DrunkenMasterCode.Cards.Ingredients;
using DrunkenMaster.DrunkenMasterCode.Tips;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Pooling;

namespace DrunkenMaster.DrunkenMasterCode.Ui;

/// <summary>
/// The pot. Three card-shaped slots above the draw pile; each Ingredient in the Brew is rendered as
/// a miniature card node in its slot, empty slots are drawn as outlines. Hovering shows the Brew tip
/// and what each ingredient will contribute.
/// </summary>
public partial class NBrewDisplay : Control
{
    private const float CardScale = 0.3f;
    private static readonly Vector2 SlotSize = NCard.defaultSize * CardScale;   // 90 x 127
    private const float Gap = 8f;
    private const float Pad = 6f;
    private const float TitleHeight = 18f;

    private readonly Player _player;
    private readonly List<NCard> _cardNodes = [];
    private readonly Control _cardLayer;
    private readonly Control _hoverCatcher;
    private readonly Label _title;

    public NBrewDisplay(Player player)
    {
        _player = player;
        Name = "BrewDisplay";
        Size = SizeFor(Cap);
        SetMouseFilter(MouseFilterEnum.Pass);

        _title = new Label
        {
            Name = "Title",
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore,
            Position = new Vector2(0, Pad - 2),
            Size = new Vector2(Size.X, TitleHeight)
        };
        _title.AddThemeFontSizeOverride("font_size", 14);
        _title.AddThemeColorOverride("font_color", new Color("FFF6E2"));
        _title.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 0.9f));
        _title.AddThemeConstantOverride("outline_size", 6);
        AddChild(_title);

        _cardLayer = new Control { Name = "Cards", MouseFilter = MouseFilterEnum.Ignore };
        _cardLayer.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(_cardLayer);

        _hoverCatcher = new Control { Name = "Hover", MouseFilter = MouseFilterEnum.Stop };
        _hoverCatcher.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _hoverCatcher.MouseEntered += OnHovered;
        _hoverCatcher.MouseExited += OnUnhovered;
        AddChild(_hoverCatcher);

        Visible = true;
    }

    public override void _Ready()
    {
        BrewSystem.Changed += OnBrewChanged;
        Refresh();
    }

    public override void _ExitTree()
    {
        BrewSystem.Changed -= OnBrewChanged;
        NHoverTipSet.Remove(_hoverCatcher);
        ClearCards();
    }

    private void OnBrewChanged(Player player)
    {
        if (player == _player) Refresh();
    }

    private Vector2 SlotOrigin(int i) => new(Pad + i * (SlotSize.X + Gap), Pad + TitleHeight);

    /// <summary>Pot size can change mid-combat (Stockpot / Shot Glass), so everything is sized per refresh.</summary>
    private int Cap => BrewSystem.CapacityFor(_player);
    private static Vector2 SizeFor(int cap) => new(Pad * 2 + SlotSize.X * cap + Gap * (cap - 1), Pad * 2 + TitleHeight + SlotSize.Y);

    public void Refresh()
    {
        ClearCards();
        var brew = BrewSystem.GetBrew(_player);
        int cap = Cap;
        Size = SizeFor(cap);
        _title.Size = new Vector2(Size.X, TitleHeight);
        _title.Text = $"Brew {Math.Min(brew.Count, cap)}/{cap}";

        for (int i = 0; i < brew.Count && i < cap; i++)
        {
            var node = NCard.Create(brew[i]);
            if (node == null) continue;
            node.MouseFilter = MouseFilterEnum.Ignore;
            _cardLayer.AddChild(node);
            node.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);
            // The NCard draws centred on its own origin, so park the origin at the slot's centre.
            node.PivotOffset = Vector2.Zero;
            node.Scale = Vector2.One * CardScale;
            node.Position = SlotOrigin(i) + SlotSize / 2f;
            _cardNodes.Add(node);
        }
        QueueRedraw();
    }

    private void ClearCards()
    {
        // NCards are pooled: detach and hand them back rather than freeing them.
        foreach (var n in _cardNodes)
        {
            if (!GodotObject.IsInstanceValid(n)) continue;
            n.GetParent()?.RemoveChild(n);
            NodePool.Free(n);
        }
        _cardNodes.Clear();
    }

    public override void _Draw()
    {
        // Panel.
        var panel = new StyleBoxFlat
        {
            BgColor = new Color(0.06f, 0.05f, 0.04f, 0.72f),
            BorderColor = new Color("8a4b12"),
            CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8, CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8
        };
        panel.SetBorderWidthAll(2);
        DrawStyleBox(panel, new Rect2(Vector2.Zero, Size));

        // Empty slot outlines.
        int cap = Cap;
        int filled = Math.Min(BrewSystem.GetBrew(_player).Count, cap);
        for (int i = 0; i < cap; i++)
        {
            var rect = new Rect2(SlotOrigin(i), SlotSize);
            var slot = new StyleBoxFlat
            {
                BgColor = i < filled ? new Color(0, 0, 0, 0) : new Color(1f, 0.85f, 0.4f, 0.08f),
                BorderColor = i < filled ? new Color(0, 0, 0, 0) : new Color("efc851") * new Color(1, 1, 1, 0.55f),
                CornerRadiusTopLeft = 5, CornerRadiusTopRight = 5, CornerRadiusBottomLeft = 5, CornerRadiusBottomRight = 5
            };
            slot.SetBorderWidthAll(2);
            DrawStyleBox(slot, rect);
        }
    }

    private void OnHovered()
    {
        var tips = new List<IHoverTip> { HoverTipFactory.Static(DrunkenMasterTips.Brew) };
        foreach (var ingredient in BrewSystem.GetBrew(_player))
        {
            tips.Add(new HoverTip(ingredient.TitleLocString, ingredient.BrewEffectText));
        }
        NHoverTipSet.CreateAndShow(_hoverCatcher, tips)?.SetGlobalPosition(GlobalPosition + new Vector2(Size.X + 8f, -40f));
    }

    private void OnUnhovered() => NHoverTipSet.Remove(_hoverCatcher);
}
