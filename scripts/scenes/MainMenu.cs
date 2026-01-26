using Godot;
using System;

public partial class MainMenu : BaseScene
{
	public Panel MenuHolder;
	public Panel LastMenu;
    public Panel HomeMenu;
    public Panel PlayMenu;
    public JukeboxPanel Jukebox;
    public Cursor Cursor;
    public MapList MapList;
    public MapInfo MapInfo;

    public Panel CurrentMenu;

    public override void _Ready()
	{
        base._Ready();

        MenuHolder = GetNode<Panel>("Menus");

        HomeMenu = MenuHolder.GetNode<Panel>("Home");
        PlayMenu = MenuHolder.GetNode<Panel>("Play");
        LastMenu = HomeMenu;

        Jukebox = GetNode<JukeboxPanel>("JukeboxPanel");
        Cursor = GetNode<Cursor>("Cursor");
        MapList = PlayMenu.GetNode<MapList>("MapList");
        MapInfo = PlayMenu.GetNode<MapInfo>("MapInfo");
		
        CurrentMenu = HomeMenu;

        Input.MouseMode = Input.MouseModeEnum.Hidden;

        foreach (Button button in CurrentMenu.GetNode("Buttons").GetChildren())
		{
			Panel menu = (Panel)MenuHolder.FindChild(button.Name, false);
			
			if (menu != null)
			{
				button.Pressed += () => { Transition(menu); };
			}
		}
    }

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
		{
			switch (mouseButton.ButtonIndex)
			{
				case MouseButton.Xbutton1:
					Transition(HomeMenu);
					break;
				case MouseButton.Xbutton2:
					Transition(LastMenu);
					break;
			}
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventKey key && key.Pressed)
		{
			switch (key.Keycode)
			{
				case Key.Space:
					if (Lobby.Map != null && CurrentMenu == PlayMenu)
					{
                        LegacyRunner.Play(Lobby.Map, Lobby.Speed, Lobby.StartFrom, Lobby.Modifiers);
                    }
                    break;
				case Key.Escape:
                    Transition(HomeMenu);
                    break;
            }
		}
	}

	public override void Load()
	{
        base.Load();
		
        Cursor.Position = GetViewport().GetMousePosition();

        DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Adaptive);

        MapInfo.InfoContainer?.Refresh();
		SceneManager.Space?.UpdateState(false);
    }

	public void Transition(Panel menu, bool instant = false)
	{
		if (CurrentMenu == menu) { return; }

		LastMenu = CurrentMenu;
		CurrentMenu = menu;

		double tweenTime = instant ? 0 : 0.15;

		Tween outTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
		outTween.TweenProperty(LastMenu, "modulate", Color.Color8(255, 255, 255, 0), tweenTime);
		outTween.TweenCallback(Callable.From(() => { LastMenu.Visible = false; }));

		CurrentMenu.Visible = true;

		Tween inTween = CreateTween().SetTrans(Tween.TransitionType.Quad);
		inTween.TweenProperty(CurrentMenu, "modulate", Color.Color8(255, 255, 255), tweenTime);
	}
}
