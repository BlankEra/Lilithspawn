using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Godot;
using Godot.NativeInterop;
using LiteDB;

public partial class Rhythia : Node
{
    private static bool loaded = false;
    private static string[] userDirectories = ["cache", "maps", "profiles", "skins", "replays", "pbs"];

    private DatabaseService databaseService = DatabaseService.Instance;

    public static Rhythia Instance;
    public static bool Quitting { get; private set; } = false;

    public override void _Ready()
    {
        Instance = this;

        GetTree().AutoAcceptQuit = false;

        // Set up user folder

        if (!File.Exists($"{Constants.USER_FOLDER}/favorites.txt"))
        {
            File.WriteAllText($"{Constants.USER_FOLDER}/favorites.txt", "");
        }

        if (!File.Exists($"{Constants.USER_FOLDER}/current_profile.txt"))
        {
            File.WriteAllText($"{Constants.USER_FOLDER}/current_profile.txt", "default");
        }

        foreach (string folder in userDirectories)
        {
            if (!Directory.Exists($"{Constants.USER_FOLDER}/{folder}"))
            {
                Directory.CreateDirectory($"{Constants.USER_FOLDER}/{folder}");
            }
        }

        foreach (string cacheFile in Directory.GetFiles($"{Constants.USER_FOLDER}/cache"))
        {
            File.Delete(cacheFile);
        }

        if (!Directory.Exists($"{Constants.USER_FOLDER}/cache/maps"))
        {
            Directory.CreateDirectory($"{Constants.USER_FOLDER}/cache/maps");
        }

        if (!Directory.Exists($"{Constants.USER_FOLDER}/skins/default"))
        {
            Directory.CreateDirectory($"{Constants.USER_FOLDER}/skins/default");
        }

        // Skin

        void deepCopy(string resDir)
        {
            string userDir = $"{Constants.USER_FOLDER}/skins/default{resDir.TrimPrefix("skin")}";

            if (!Directory.Exists(userDir))
            {
                Directory.CreateDirectory(userDir);
            }

            foreach (string resFile in Godot.DirAccess.GetFilesAt($"res://{resDir}"))
            {
                string userFile = $"{userDir}/{resFile}";
                string ext = resFile.GetExtension();

                if (ext == "import" || ext == "uid" || File.Exists(userFile))
                {
                    continue;
                }

                Godot.FileAccess source = Godot.FileAccess.Open($"res://{resDir}/{resFile}", Godot.FileAccess.ModeFlags.Read);
                byte[] buffer = source.GetBuffer((long)source.GetLength());
                source.Close();

                Godot.FileAccess copy = Godot.FileAccess.Open(userFile, Godot.FileAccess.ModeFlags.Write);
                copy.StoreBuffer(buffer);
                copy.Close();
            }

            foreach (string dir in Godot.DirAccess.GetDirectoriesAt($"res://{resDir}"))
            {
                deepCopy($"{resDir}/{dir}");
            }
        }

        deepCopy("skin");

        // Settings

        if (!File.Exists($"{Constants.USER_FOLDER}/profiles/default.json"))
        {
            SettingsManager.Save("default");
        }

        try
        {
            SettingsManager.Load();
        }
        catch
        {
            SettingsManager.Save();
        }

        // Stats

        if (!File.Exists($"{Constants.USER_FOLDER}/stats"))
        {
            Logger.Log("Stats file not found");
            File.WriteAllText($"{Constants.USER_FOLDER}/stats", "");
            Stats.Save();
        }

        try
        {
            Stats.Load();
        }
        catch
        {
            Stats.GamePlaytime = 0;
            Stats.TotalPlaytime = 0;
            Stats.GamesOpened = 0;
            Stats.TotalDistance = 0;
            Stats.NotesHit = 0;
            Stats.NotesMissed = 0;
            Stats.HighestCombo = 0;
            Stats.Attempts = 0;
            Stats.Passes = 0;
            Stats.FullCombos = 0;
            Stats.HighestScore = 0;
            Stats.Total_Score = 0;
            Stats.RageQuits = 0;
            Stats.PassAccuracies = [];
            Stats.FavoriteMaps = [];

            Stats.Save();
        }

        Stats.GamesOpened++;

        List<string> import = [];

        foreach (string file in Directory.GetFiles($"{Constants.USER_FOLDER}/maps"))
        {
            if (file.GetExtension() == "sspm" || file.GetExtension() == "txt")
            {
                import.Add(file);
            }
        }

        MapParser.BulkImport([.. import]);

        foreach (string file in import)
        {
            File.Delete(file);
        }

        loaded = true;
    }

    public static void Quit()
    {
        var settings = SettingsManager.Instance.Settings;

        if (Quitting)
        {
            return;
        }

        Quitting = true;

        if (!LegacyRunner.CurrentAttempt.IsReplay)
        {
            LegacyRunner.CurrentAttempt.Stop();
        }

        Stats.TotalPlaytime += (Time.GetTicksUsec() - Constants.STARTED) / 1000000;

        if (loaded)
        {
            SettingsManager.Save();
            Stats.Save();
        }

        if (File.Exists($"{Constants.USER_FOLDER}/maps/NA_tempmap.phxm"))
        {
            File.Delete($"{Constants.USER_FOLDER}/maps/NA_tempmap.phxm");
        }

        Discord.Client.Dispose();
        DatabaseService.Instance.Dispose();
        
        Instance.GetTree().Quit();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            if (SceneManager.Scene.Name == "SceneGame")
            {
                Stats.RageQuits++;
            }

            Quit();
        }
    }
}