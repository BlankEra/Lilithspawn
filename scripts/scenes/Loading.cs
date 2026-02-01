using Godot;
using System;

public partial class Loading : BaseScene
{
    private ColorRect background;
    private TextureRect splash;
    private TextureRect splashShift;

    public override void _Ready()
    {
        base._Ready();

        background = GetNode<ColorRect>("Background");
        splash = GetNode<TextureRect>("Splash");
        splashShift = GetNode<TextureRect>("SplashShift");

        Tween inTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetParallel();
        inTween.TweenProperty(background, "color", Color.FromHtml("#060509"), 1);
        inTween.TweenProperty(splash, "modulate", Color.Color8(255, 255, 255, 255), 0.5);
        inTween.TweenProperty(splashShift, "modulate", Color.Color8(255, 255, 255, 255), 0.25);
        inTween.SetTrans(Tween.TransitionType.Sine);
        inTween.Chain().TweenProperty(splashShift, "modulate", Color.Color8(255, 255, 255, 0), 2.5);

        inTween.Chain().TweenCallback(Callable.From(() => {
            if (MapManager.Initialized)
            {
                Exit();
            }
            else
            {
                MapManager.MapsInitialized += _ => Exit();
            }
        }));
    }

    public void Exit()
    {
        Tween outTween = CreateTween().SetTrans(Tween.TransitionType.Quad).SetParallel();
        outTween.TweenProperty(background, "color", Color.Color8(0, 0, 0, 255), 0.5);
        outTween.TweenProperty(splash, "modulate", Color.Color8(0, 0, 0, 255), 0.5);
        outTween.Chain().TweenCallback(Callable.From(() => { SceneManager.Load("res://scenes/main_menu.tscn"); }));
    }
}
