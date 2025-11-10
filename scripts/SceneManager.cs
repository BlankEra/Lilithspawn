using Godot;

public partial class SceneManager : Node
{

    private static SubViewportContainer backgroundContainer;

    private static SubViewport backgroundViewport;

    private static Node3D space;

    private static string activeScenePath;

    private static bool skipNextTransition = false;

    public static Node Node { get; private set; }

    public static Node Scene;

    public override void _Ready()
    {
        if (Name != "Main")
        {
            return;
        }

        Node = this;
        backgroundContainer = Node.GetNode<SubViewportContainer>("Background");

        backgroundViewport = backgroundContainer.GetNode<SubViewport>("SubViewport");

        space = backgroundContainer.GetNode<Node3D>("Waves");

        Load("res://scenes/loading.tscn", true);

        Node.GetTree().Connect("node_added", Callable.From((System.Action<Node>)((Node child) =>
        {
            if (child.Name != "SceneMenu" && child.Name != "SceneGame" && child.Name != "SceneResults")
            {
                return;
            }

            if (skipNextTransition)
            {
                skipNextTransition = false;
                return;
            }

            ColorRect inTransition = SceneManager.Scene.GetNode<ColorRect>("Transition");
            inTransition.SelfModulate = Color.FromHtml("ffffffff");
            var inTween = inTransition.CreateTween();
            inTween.TweenProperty(inTransition, "self_modulate", Color.FromHtml("ffffff00"), 0.25).SetTrans(Tween.TransitionType.Quad);
            inTween.Play();
        })));
    }

    public static void ReloadCurrentScene()
    {
        Load(activeScenePath);
    }

    public static void Load(string path, bool skipTransition = false)
    {

        if (skipTransition)
        {
            skipNextTransition = true;
            swapScene(path);
        }
        else
        {
            ColorRect outTransition = Scene.GetNode<ColorRect>("Transition");
            Tween outTween = outTransition.CreateTween();
            outTween.TweenProperty(outTransition, "self_modulate", Color.FromHtml("ffffffff"), 0.25).SetTrans(Tween.TransitionType.Quad);
            outTween.TweenCallback(Callable.From(() =>
            {
                swapScene(path);
            }));
            outTween.Play();
        }
    }

    private static void swapScene(string path)
    {
        if (Scene != null && Scene.GetParent() != null)
        {
            Node.RemoveChild(Scene);
        }

        var node = ResourceLoader.Load<PackedScene>(path).Instantiate();

        // temporary background space fix
        if (Scene != null)
        {
            if (Scene.Name == "SceneMenu")
            {
                backgroundContainer.RemoveChild(space);
            } else if (space.GetParent() != backgroundContainer) {
                backgroundContainer.AddChild(space);
            }
        }
        //

        activeScenePath = path;
        Scene = node;
        Node.AddChild(node);
    }
}
