using Godot;
using System;

public class GhostMod : Mod, IObjectRenderModifier<Note>
{
    public override string Name => "Ghost";

    public override bool Rankable => true;

    public override double ScoreMultiplier => 1.03;

    public void ApplyRenderObject(Note note, Color color, float depth, Attempt attempt)
    {
        float ad = (float)attempt.Settings.ApproachDistance;

        // TODO: This won't work since Color is a struct, add a Transparency field to the note object instead
        color.A -= Mathf.Min(1, (ad - depth) / (ad / 2));
    }
}
