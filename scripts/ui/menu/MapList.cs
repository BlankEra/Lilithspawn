using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class MapList : Panel
{
	[Export]
    public float ButtonSize = 90;
	[Export]
    public float ButtonHoverSize = 10;
	[Export]
    public float ButtonSelectedSize = 20;
	[Export]
    public float Spacing = 12;
	[Export]
    public float ScrollStep = 1500;
	[Export]
    public float ScrollFriction = 20;
	[Export]
    public float ScrollElasticity = 0.02f;

    public double ScrollLength = 0;
    public double ScrollMomentum = 0;
    public double TargetScroll = 0;
    public double Scroll = 0;
    public bool MouseScroll = false;
    public bool DisplaySelectionCursor = false;

    [Signal]
    public delegate void OnMapSelectedEventHandler(Map map);

    private TextureRect mask;
    private TextureRect selectionCursor;
    private Panel scrollBar;
    private Panel scrollBarMain;
    private TextureRect scrollBarMainTop;
    private TextureRect scrollBarMainMiddle;
    private TextureRect scrollBarMainBottom;
    private Panel scrollBarBackground;
    private TextureRect scrollBarBackgroundTop;
    private TextureRect scrollBarBackgroundMiddle;
    private TextureRect scrollBarBackgroundBottom;

    private readonly PackedScene mapButtonTemplate = ResourceLoader.Load<PackedScene>("res://prefabs/map_button.tscn");
    private List<Map> maps = [];	// (queried) map IDs, get this from db in the future
    private Dictionary<string, MapButton> mapButtons = [];
    private Stack<MapButton> mapButtonCache = [];
    private MapButton hoveredButton;
    private string selectedMapID;

    public override void _Ready()
    {
        mask = GetNode<TextureRect>("Mask");
        selectionCursor = GetNode<TextureRect>("SelectionCursor");
        scrollBar = GetNode<Panel>("ScrollBar");
        scrollBarMain = scrollBar.GetNode<Panel>("Main");
        scrollBarMainTop = scrollBarMain.GetNode<TextureRect>("Top");
        scrollBarMainMiddle = scrollBarMain.GetNode<TextureRect>("Middle");
        scrollBarMainBottom = scrollBarMain.GetNode<TextureRect>("Bottom");
        scrollBarBackground = scrollBar.GetNode<Panel>("Background");
        scrollBarBackgroundTop = scrollBarBackground.GetNode<TextureRect>("Top");
        scrollBarBackgroundMiddle = scrollBarBackground.GetNode<TextureRect>("Middle");
        scrollBarBackgroundBottom = scrollBarBackground.GetNode<TextureRect>("Bottom");

        MouseExited += () => { toggleSelectionCursor(false); };
        SkinManager.Instance.OnLoaded += updateSkin;

        updateSkin();

        // temporary until db is implemented
        foreach (string path in Directory.GetFiles($"{Constants.USER_FOLDER}/maps"))
		{
            maps.Add(MapParser.Decode(path, null, false));
        }
    }

    public override void _Process(double delta)
    {
        float scrollElasticOffset = 0;

		if ((TargetScroll <= 0 && ScrollMomentum < 0) || (TargetScroll >= ScrollLength && ScrollMomentum > 0))
		{
            scrollElasticOffset = (float)(ScrollMomentum * ScrollElasticity);
        }

        ScrollLength = Math.Max(0, maps.Count * (ButtonSize + Spacing) - Spacing - Size.Y) + ButtonHoverSize + ButtonSelectedSize;
        ScrollMomentum = Mathf.Lerp(ScrollMomentum, 0.0, Math.Min(1, ScrollFriction * delta));

		if (MouseScroll)
		{
            float t = Mathf.InverseLerp(Position.Y + scrollBarMain.Size.Y / 2, Position.Y + Size.Y - scrollBarMain.Size.Y / 2, GetViewport().GetMousePosition().Y);
            TargetScroll = Mathf.Lerp(TargetScroll, ScrollLength * Math.Clamp(t, 0, 1), Math.Min(1, 24 * delta));
        }
		else
		{
            TargetScroll = Math.Clamp(TargetScroll + ScrollMomentum * delta, 0, ScrollLength) + scrollElasticOffset;
        }

        Scroll = Mathf.Lerp(Scroll, TargetScroll, Math.Min(1, 20 * delta));

        scrollBarMain.AnchorTop = (float)Math.Max(0, (TargetScroll - scrollElasticOffset) / (ScrollLength + Size.Y));
        scrollBarMain.AnchorBottom = (float)Math.Min(1, scrollBarMain.AnchorTop + Size.Y / (ScrollLength + Size.Y));

        List<MapButton> drawnButtons = [];
        List<float> buttonSizeOffsets = [];
        float upOffset = 0;
        float downOffset = 0;
        int mapCount = -1;

        foreach (Map map in maps)
		{
			mapCount++;

            float offset = mapCount * (ButtonSize + Spacing);
            float top = offset - (float)Scroll;
            float bottom = top + ButtonSize;
            bool display = top < Size.Y && bottom > 0;
            MapButton button = mapButtons.TryGetValue(map.ID, out MapButton value) ? value : null;
			
            // Cache/ignore if outside map list
            if (!display)
			{
				if (button != null)
				{
                    mapButtons.Remove(map.ID);
                    mapButtonCache.Push(button);
                    mask.RemoveChild(button);

                    button.StickoutOffset = 0;
                    button.Deselect();
                    button.UpdateOutline(0f, 0f);

					if (button == hoveredButton)
					{
                        hoveredButton = null;
                    }
                }

                continue;
            }

			// Everything must be rendered from here

			if (button == null)
			{
				button = mapButtonCache.Count > 0 ? mapButtonCache.Pop() : setupButton(mapButtonTemplate.Instantiate<MapButton>());

                button.ListIndex = mapCount;
                mapButtons[map.ID] = button;
                mask.AddChild(button);
                button.UpdateInfo(map);

				if (map.ID == selectedMapID)
				{
                    button.Select();
                    button.UpdateOutline(1f, 0f);
                }
            }

            float sizeOffset = button.Size.Y - button.SizeHeight;

            downOffset += sizeOffset;
            drawnButtons.Add(button);
            buttonSizeOffsets.Add(sizeOffset);
        }

        Vector2 selectionCursorPos = new(
            hoveredButton != null && DisplaySelectionCursor ? hoveredButton.Position.X - 60 : -80,
            Math.Clamp(hoveredButton != null ? hoveredButton.Position.Y + hoveredButton.Size.Y / 2 - selectionCursor.Size.Y / 2 : selectionCursor.Position.Y, 0, Size.Y)
        );

        selectionCursor.Position = selectionCursor.Position.Lerp(selectionCursorPos, (float)Math.Min(1, 8 * delta));
        selectionCursor.Rotation = -selectionCursor.Position.Y / 60;

        for (int i = 0; i < drawnButtons.Count; i++)
		{
            MapButton button = drawnButtons[i];
            float sizeOffset = buttonSizeOffsets[i];

            float indexOffset = button.ListIndex * (ButtonSize + Spacing);
            float top = indexOffset - (float)Scroll - sizeOffset / 2;

            downOffset -= sizeOffset;
            top += (upOffset - downOffset) / 2 + (ButtonHoverSize + ButtonSelectedSize) / 2;
            upOffset += sizeOffset;

            float centerOffset = Math.Abs((top + button.Size.Y / 2) - Size.Y / 2) / (Size.Y / 2 + ButtonSize / 2);

            button.CenterOffset = centerOffset;
            button.ZIndex = button.ListIndex == 0 || button.ListIndex == maps.Count - 1 ? 1 : 0;
            button.Position = new(button.Position.X, top);
            button.OutlineShader.SetShaderParameter("cursor_position", GetViewport().GetMousePosition());
            button.OutlineShader.SetShaderParameter("light_position", DisplaySelectionCursor ? selectionCursor.GlobalPosition : Vector2.Left * 10000);
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && !mouseButton.CtrlPressed)
		{
			switch (mouseButton.ButtonIndex)
			{
				case MouseButton.Right: MouseScroll = mouseButton.Pressed; break;
				case MouseButton.WheelDown: ScrollMomentum += ScrollStep; break;
				case MouseButton.WheelUp: ScrollMomentum -= ScrollStep; break;
            }
		}
    }

	private MapButton setupButton(MapButton button)
	{
        button.SizeHeight = ButtonSize;
        button.HoverSizeOffset = ButtonHoverSize;
        button.SelectedSizeOffset = ButtonSelectedSize;
        button.CustomMinimumSize = new(0, ButtonSize);

        button.MouseEntered += () => {
            hoveredButton = button;

            toggleSelectionCursor(true);

			if (button.Map.ID != selectedMapID)
			{
                button.UpdateOutline(0.5f);
            }
        };
        button.MouseExited += () => {
            if (button.Map.ID != selectedMapID)
            {
                button.UpdateOutline(0f);
            }
        };
        button.OnPressed += () => {
			if (selectedMapID != null && selectedMapID != button.Map.ID && mapButtons.TryGetValue(selectedMapID, out MapButton value))
			{
                value.Deselect();
                value.UpdateOutline(0f);
            }

            selectedMapID = button.Map.ID;
            EmitSignal(SignalName.OnMapSelected, button.Map);

            button.UpdateOutline(1.0f);
        };

        return button;
    }

	private void toggleSelectionCursor(bool display)
	{
        if (DisplaySelectionCursor == display) { return; }
        
        DisplaySelectionCursor = display;

		if (display && hoveredButton != null)
		{
            selectionCursor.Position = new(selectionCursor.Position.X, hoveredButton.Position.Y);
        }

        Tween tween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetParallel();
        tween.TweenProperty(selectionCursor, "modulate", Color.Color8(255, 255, 255, (byte)(display ? 255 : 0)), 0.1);
    }

    private void updateSkin()
    {
        mask.Texture = SkinManager.Instance.Skin.MapListMaskImage;
        selectionCursor.Texture = SkinManager.Instance.Skin.MapListSelectionCursorImage;
        scrollBarMainTop.Texture = SkinManager.Instance.Skin.MapListScrollBarTopImage;
        scrollBarMainMiddle.Texture = SkinManager.Instance.Skin.MapListScrollBarMiddleImage;
        scrollBarMainBottom.Texture = SkinManager.Instance.Skin.MapListScrollBarBottomImage;
        scrollBarBackgroundTop.Texture = SkinManager.Instance.Skin.MapListScrollBarBackgroundTopImage;
        scrollBarBackgroundMiddle.Texture = SkinManager.Instance.Skin.MapListScrollBarBackgroundMiddleImage;
        scrollBarBackgroundBottom.Texture = SkinManager.Instance.Skin.MapListScrollBarBackgroundBottomImage;
    }
}