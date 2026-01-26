using Godot;
using System;

public partial class FlatPreview : Panel
{
    public Map Map;
    public bool Playing = false;
    public double Time = 0;
    public double Speed = 1;

    public override void _Process(double delta)
    {
        if (!Playing) { return; }

		Time += delta * Speed;
    }

	public void Setup(Map map)
	{
        Map = map;
    }

	public void Seek(double seek)
	{
        Time = seek;
    }
}