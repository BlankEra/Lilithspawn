/*
	TEMPORARILY C# TO MAKE IT WORK
*/

using Godot;
using System;
using System.IO;

public partial class RandomButton : Button
{
	public override void _Pressed()
	{
		string[] mapPool = Directory.GetFiles($"{Constants.USER_FOLDER}/maps");
		string map = mapPool[new Random().Next(mapPool.Length)];
		
		SceneManager.Load("res://scenes/game.tscn");
		LegacyRunner.Play(MapParser.Decode(map), Lobby.Speed, Lobby.StartFrom, Lobby.Mods);
	}
}
