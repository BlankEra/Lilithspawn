using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[GlobalClass]
public partial class MapManager : Node
{
    public System.Collections.Generic.Dictionary<string, Map> Maps = new();

    public static List<string> GetFavorites()
    {
        return [.. File.ReadAllText($"{Constants.USER_FOLDER}/favorites.txt").Split("\n")];
    }

    public static bool IsFavorited(Map map)
    {
        return GetFavorites().Contains(map.ID);
    }
}
