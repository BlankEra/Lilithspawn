using Godot;
using System;

public partial class FPSCounter : Label
{
    public uint Frames = 0;

    private double time = 0;

    public override void _Process(double delta)
    {
        Frames++;
        time += delta;

		if (time >= 1)
		{
            Text = $"{Frames} FPS";
			
            time--;
            Frames = 0;
        }
    }
}