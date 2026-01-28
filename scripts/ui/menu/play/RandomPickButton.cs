using Godot;
using System;
using System.IO;

public partial class RandomPickButton : Button
{
    public override void _Pressed() { Pick(); }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && key.CtrlPressed)
		{
			switch (key.Keycode)
			{
				case Key.F4:
                    Pick();
                    break;
            }
		}
    }

	public void Pick()
	{
		string[] mapPool = Directory.GetFiles($"{Constants.USER_FOLDER}/maps");
        string map = mapPool[new Random().Next(mapPool.Length)];
		
		LegacyRunner.Play(MapParser.Decode(map), Lobby.Speed, Lobby.StartFrom, Lobby.Modifiers);
	}
}