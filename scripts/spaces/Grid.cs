using Godot;
using System;

namespace Spaces;

public partial class Grid : BaseSpace
{
    public Color Color = Color.Color8(255, 255, 255, 255);

    private StandardMaterial3D tileMaterial;
    private WorldEnvironment environment;

    public override void _Ready()
    {
        base._Ready();
        
        tileMaterial = (GetNode<MeshInstance3D>("Top").Mesh as PlaneMesh).Material as StandardMaterial3D;
        environment = GetNode<WorldEnvironment>("WorldEnvironment");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Playing)
        {
            Camera.Transform = LegacyRunner.Camera.Transform;
        }

        Color = Color.Lerp(LegacyRunner.CurrentAttempt.LastHitColour, (float)delta * 8);

        tileMaterial.AlbedoColor = Color;
        tileMaterial.Uv1Offset += Vector3.Up * (float)delta * 3;
    }
}