using System.Collections.Generic;
using Godot;

public partial class SceneManager : Node
{
    private static SubViewportContainer backgroundContainer;

    private static SubViewport backgroundViewport;

    private static string activeScenePath;

    public static SceneManager Instance { get; private set; }

    public static Window Root;

    public static Dictionary<string, BaseScene> Scenes = [];

    public static BaseScene Scene;

    public static BaseSpace Space;

    public static Panel VolumePanel;

    public override void _EnterTree()
    {
        Instance = this;
        Root = GetTree().Root;
        VolumePanel = GetNode<Panel>("Volume");
    }

    public override void _Ready()
    {
        backgroundContainer = GetNode<SubViewportContainer>("Background");
        backgroundViewport = backgroundContainer.GetNode<SubViewport>("SubViewport");

        Load("res://scenes/loading.tscn");
    }

    public static void ReloadCurrentScene()
    {
        Load(activeScenePath, true);
    }

    public static void Load(string path, bool skipTransition = false)
    {
        bool isSceneLoaded = Scenes.TryGetValue(path, out BaseScene loadedScene);
        BaseScene newScene = isSceneLoaded ? loadedScene : (BaseScene)ResourceLoader.Load<PackedScene>(path).Instantiate();

        //                  temp solution until these scenes are non-static
        if (!isSceneLoaded && newScene.Name != "SceneGame" && newScene.Name != "SceneResults")
        {
            Scenes[path] = newScene;
        }

        Tween outTween = Instance.CreateTween().SetTrans(Tween.TransitionType.Quad);
        
        if (Scene != null)
        {
            outTween.TweenProperty(Scene.Transition, "self_modulate", Color.FromHtml("ffffffff"), skipTransition ? 0 : 0.25);
        }

        outTween.TweenCallback(Callable.From(() => {
            removeScene(Scene);
            addScene(newScene);

            newScene.Transition.SelfModulate = Color.FromHtml("ffffffff");
            Instance.CreateTween().SetTrans(Tween.TransitionType.Quad).TweenProperty(newScene.Transition, "self_modulate", Color.FromHtml("ffffff00"), skipTransition ? 0 : 0.25);

            activeScenePath = path;
            Scene = newScene;
        }));
    }

    private static void addScene(BaseScene scene, bool updateSpace = true)
    {
        if (scene == null || scene.GetParent() == Instance) { return; }

        if (updateSpace)
        {
            addSpace(scene.GetSpace());
        }
        
        Instance.AddChild(scene);
        scene.Load();
    }

    private static void removeScene(BaseScene scene, bool updateSpace = true)
    {
        if (scene == null || scene.GetParent() != Instance) { return; }

        scene.Unload();
        Instance.RemoveChild(scene);

        // also temp
        if (scene.Name == "SceneGame" || scene.Name == "SceneResults")
        {
            scene.QueueFree();
        }

        if (updateSpace)
        {
            removeSpace();
        }
    }

    private static void addSpace(BaseSpace space)
    {
        if (space == null || space.GetParent() == backgroundViewport) { return; }

        backgroundViewport.AddChild(space);

        Space = space;
    }

    private static void removeSpace()
    {
        if (Space == null || Space.GetParent() != backgroundViewport) { return; }

        backgroundViewport.RemoveChild(Space);

        Space = null;
    }
}
