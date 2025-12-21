using Godot;
using System;
using System.IO;

public partial class MapInfoContainer : Panel
{
	/// <summary>
    /// Parsed map reference
    /// </summary>
    public Map Map;
    public Leaderboard Leaderboard = new();

    private readonly PackedScene leaderboardScoreTemplate = ResourceLoader.Load<PackedScene>("res://prefabs/score_panel.tscn");
    private Panel info;
    private Panel actions;
    private Panel leaderboard;
    private ColorRect dim;
    private TextureRect coverBackground;
    private TextureRect cover;
    private RichTextLabel mainLabel;
    private Label extraLabel;
    private ScrollContainer lbScrollContainer;
    private VBoxContainer lbContainer;
    private Button lbExpand;
    private Button lbHide;
    private ShaderMaterial outlineMaterial;

    public override void _Ready()
	{
        info = GetNode<Panel>("Info");
        actions = GetNode<Panel>("Actions");
        leaderboard = GetNode<Panel>("Leaderboard");
        dim = GetNode<ColorRect>("Dim");
        coverBackground = info.GetNode("CoverContainer").GetNode<TextureRect>("Background");
        cover = coverBackground.GetNode<TextureRect>("Cover");
        mainLabel = info.GetNode<RichTextLabel>("MainLabel");
        extraLabel = info.GetNode<Label>("Extra");
        lbScrollContainer = leaderboard.GetNode<ScrollContainer>("ScrollContainer");
        lbContainer = lbScrollContainer.GetNode<VBoxContainer>("VBoxContainer");
        lbExpand = leaderboard.GetNode<Button>("Expand");
        lbHide = GetNode<Button>("LeaderboardHide");
        outlineMaterial = info.GetNode<Panel>("Outline").Material as ShaderMaterial;

        SkinManager.Instance.OnLoaded += updateSkin;

        Panel lbExpandHover = lbExpand.GetNode<Panel>("Hover");

        void tweenExpandHover(bool show)
		{
            CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quart).TweenProperty(lbExpandHover, "modulate", Color.Color8(255, 255, 255, (byte)(show ? 255 : 0)), 0.25);
        }

        lbExpand.MouseEntered += () => { tweenExpandHover(true); };
		lbExpand.MouseExited += () => { tweenExpandHover(false); };
        lbExpand.Pressed += () => { toggleLeaderboard(true); };
        lbHide.Pressed += () => { toggleLeaderboard(false); };

        info.OffsetLeft -= 64;
		info.OffsetRight -= 64;
		actions.OffsetLeft -= 80;
		actions.OffsetRight -= 80;
		leaderboard.OffsetLeft -= 96;
		leaderboard.OffsetRight -= 96;

        Tween inTween = CreateTween().SetEase(Tween.EaseType.Out).SetParallel();
        inTween.SetTrans(Tween.TransitionType.Quint).TweenProperty(info, "offset_left", 0, 0.5);
		inTween.TweenProperty(info, "offset_right", 0, 0.5);
		inTween.SetTrans(Tween.TransitionType.Quart).TweenProperty(actions, "offset_left", 0, 0.6);
		inTween.TweenProperty(actions, "offset_right", 0, 0.6);
		inTween.SetTrans(Tween.TransitionType.Cubic).TweenProperty(leaderboard, "offset_left", 0, 0.7);
		inTween.TweenProperty(leaderboard, "offset_right", 0, 0.7);

        OffsetRight = 0;
        Position += Vector2.Left * 64;
        Modulate = Color.Color8(255, 255, 255, 0);
    }

	public override void _Process(double delta)
    {
        outlineMaterial.SetShaderParameter("cursor_position", GetViewport().GetMousePosition());
    }

	public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
		{
			switch (mouseButton.ButtonIndex)
			{
				case MouseButton.Right: toggleLeaderboard(false); break;
            }
		}
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
		{
			switch (mouseButton.ButtonIndex)
			{
				case MouseButton.Left: toggleLeaderboard(false); break;
            }
		}
    }

	public void Setup(Map map)
	{
        Map = map;
        Name = map.ID;

		// until covers are lazyloaded
        if (map.CoverBuffer != null)
		{
			Godot.FileAccess file = Godot.FileAccess.Open($"{Constants.USER_FOLDER}/cache/info_cover{map.ID}.png", Godot.FileAccess.ModeFlags.WriteRead);
			file.StoreBuffer(map.CoverBuffer);
			file.Close();
			
			ImageTexture tex = ImageTexture.CreateFromImage(Image.LoadFromFile($"{Constants.USER_FOLDER}/cache/info_cover{map.ID}.png"));

			cover.Texture = tex;
        }
		//

        mainLabel.Text = string.Format(mainLabel.Text, map.PrettyTitle, Constants.DIFFICULTY_COLORS[map.Difficulty].ToHtml(), map.DifficultyName, map.PrettyMappers);
        extraLabel.Text = string.Format(extraLabel.Text, Util.String.FormatTime(map.Length / 1000), map.Notes.Length);

        if (File.Exists($"{Constants.USER_FOLDER}/pbs/{map.ID}"))
		{
			Leaderboard = new(map.ID, $"{Constants.USER_FOLDER}/pbs/{map.ID}");
		}

		if (!Leaderboard.Valid || Leaderboard.ScoreCount == 0)
		{
            leaderboard.Visible = false;
        }
		else
		{
            for (int i = 0; i < Math.Min(8, Leaderboard.ScoreCount); i++)
            {
                ScorePanel panel = leaderboardScoreTemplate.Instantiate<ScorePanel>();
				
                lbContainer.AddChild(panel);
                panel.Setup(Leaderboard.Scores[i]);
                panel.GetNode<ColorRect>("Background").Color = Color.Color8(255, 255, 255, (byte)(i % 2 == 0 ? 0 : 8));

                panel.Button.Pressed += () => { toggleLeaderboard(false); };
            }
        }
    }

	public Tween Transition(bool show)
	{
        Tween tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic).SetParallel();
        float time = show ? 0.4f : 0.3f;

        PivotOffset = Size / 2;

        tween.TweenProperty(this, "modulate", Color.Color8(255, 255, 255, (byte)(show ? 255 : 0)), time);
        tween.TweenProperty(this, "position", show ? Vector2.Zero : Vector2.Down * 24, time);
        tween.TweenProperty(this, "scale", Vector2.One * (show ? 1f : 0.9f), time);
        tween.Chain();
		
        return tween;
    }

	private void toggleLeaderboard(bool show)
	{
        lbExpand.Visible = !show;
        lbHide.Visible = show;
        lbScrollContainer.VerticalScrollMode = show ? ScrollContainer.ScrollMode.Auto : ScrollContainer.ScrollMode.ShowNever;

        foreach (ScorePanel panel in lbContainer.GetChildren())
		{
            panel.Button.Visible = show;
        }

        Tween tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quart).SetParallel();

        tween.TweenProperty(leaderboard, "offset_top", -100 * (show ? Math.Min(4, Leaderboard.ScoreCount) : 1), 0.25);
        tween.TweenProperty(dim, "color", Color.Color8(0, 0, 0, (byte)(show ? 128 : 0)), 0.25);
    }

	private void updateSkin()
	{
        coverBackground.Texture = SkinManager.Instance.Skin.MapInfoCoverBackgroundImage;
    }
}
