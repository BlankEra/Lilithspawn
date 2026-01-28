using Godot;
using System.IO;

public partial class BaseSpace : Node3D
{
    public bool Playing = false;

    public Camera3D Camera;
    public WorldEnvironment WorldEnvironment;
    public ImageTexture Cover;

    public override void _Ready()
    {
        base._Ready();

        Camera = GetNode<Camera3D>("Camera3D");
        WorldEnvironment = GetNode<WorldEnvironment>("WorldEnvironment");
    }

    public virtual void UpdateMap(Map map)
    {
        Cover = null;
    }

    public virtual void UpdateState(bool playing)
    {
        Playing = playing;
    }
}