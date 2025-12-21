using Godot;
using System;

public partial class MainMenu : Control
{
    public Panel MenuHolder;
    public Panel CurrentMenu;

    private Panel lastMenu;

    public override void _Ready()
    {
        MenuHolder = GetNode<Panel>("Menus");
		CurrentMenu = MenuHolder.GetNode<Panel>("Main");
        lastMenu = CurrentMenu;

        Input.MouseMode = Input.MouseModeEnum.Hidden;
        DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Adaptive);

        foreach (Button button in CurrentMenu.GetNode("Buttons").GetChildren())
		{
            Panel menu = (Panel)MenuHolder.FindChild(button.Name, false);
			
			if (menu != null)
			{
                button.Pressed += () => { Transition(menu); };
            }
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
		{
			switch (mouseButton.ButtonIndex)
			{
				case MouseButton.Xbutton1: Transition(MenuHolder.GetNode<Panel>("Main")); break;
				case MouseButton.Xbutton2: Transition(lastMenu); break;
            }
		}
    }

	public void Transition(Panel menu, bool instant = false)
	{
		if (CurrentMenu == menu) { return; }

        lastMenu = CurrentMenu;
        CurrentMenu = menu;

        double tweenTime = instant ? 0 : 0.15;

        Tween outTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        outTween.TweenProperty(lastMenu, "modulate", Color.Color8(255, 255, 255, 0), tweenTime);
        outTween.TweenCallback(Callable.From(() => { lastMenu.Visible = false; }));

        CurrentMenu.Visible = true;

		Tween inTween = CreateTween().SetTrans(Tween.TransitionType.Quad);
        inTween.TweenProperty(CurrentMenu, "modulate", Color.Color8(255, 255, 255), tweenTime);
    }
}