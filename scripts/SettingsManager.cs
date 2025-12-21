using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class SettingsManager : Node
{
    public static bool Shown = false;
    public static ColorRect Menu;

    public static SettingsManager Instance { get; private set; }

    public SettingsProfile Settings = new SettingsProfile();

    [Signal]
    public delegate void OnShownEventHandler(bool shown);

    public override void _Ready()
    {
        Instance = this;

        Menu = GD.Load<PackedScene>("res://prefabs//settings.tscn").Instantiate<ColorRect>();
        
        HideMenu();
    }

    public static void ShowMenu(bool show = true)
    {
        Shown = show;

        Instance.EmitSignal(SignalName.OnShown, Shown);
    }

    public static void HideMenu()
    {
        ShowMenu(false);
    }

    // public static void ApplySetting(string setting, Variant value)
    // {
    //     switch (setting)
    //     {
    //         case "Sensitivity":
    //             Instance.Settings.Sensitivity.Value = (float)value;
    //             break;
    //         case "ApproachRate":
    //             Instance.Settings.ApproachRate.Value = (float)value;
    //             break;
    //         case "ApproachDistance":
    //             Instance.Settings.ApproachDistance.Value = (float)value;
    //             break;
    //         case "FadeIn":
    //             Instance.Settings.FadeIn.Value = (float)value;
    //             break;
    //         case "Parallax":
    //             Instance.Settings.Parallax.Value = (float)value;
    //             break;
    //         case "FoV":
    //             Instance.Settings.FoV.Value = (float)value;
    //             break;
    //         case "VolumeMaster":
    //             Instance.Settings.VolumeMaster.Value = (float)value;
    //             break;
    //         case "VolumeMusic":
    //             Instance.Settings.VolumeMusic.Value = (float)value;
    //             break;
    //         case "VolumeSFX":
    //             Instance.Settings.VolumeSFX.Value = (float)value;
    //             break;
    //         case "AlwaysPlayHitSound":
    //             Instance.Settings.AlwaysPlayHitSound.Value = (bool)value;
    //             break;
    //         case "NoteSize":
    //             Instance.Settings.NoteSize.Value = (float)value;
    //             break;
    //         case "CursorScale":
    //             Instance.Settings.CursorScale.Value = (float)value;
    //             break;
    //         case "FadeOut":
    //             Instance.Settings.FadeOut.Value = (bool)value;
    //             break;
    //         case "Pushback":
    //             Instance.Settings.Pushback.Value = (bool)value;
    //             break;
    //         case "Fullscreen":
    //             Instance.Settings.Fullscreen.Value = (bool)value;
    //             break;
    //         case "CursorTrail":
    //             Instance.Settings.CursorTrail.Value = (bool)value;
    //             break;
    //         case "TrailTime":
    //             Instance.Settings.TrailTime.Value = (float)value;
    //             break;
    //         case "TrailDetail":
    //             Instance.Settings.TrailDetail.Value = (float)value;
    //             break;
    //         case "CursorDrift":
    //             Instance.Settings.CursorDrift.Value = (bool)value;
    //             break;
    //         case "VideoDim":
    //             Instance.Settings.VideoDim.Value = (float)value;
    //             break;
    //         case "VideoRenderScale":
    //             Instance.Settings.VideoRenderScale.Value = (float)value;
    //             break;
    //         case "SimpleHUD":
    //             Instance.Settings.SimpleHUD.Value = (bool)value;
    //             break;
    //         case "AutoplayJukebox":
    //             Instance.Settings.AutoplayJukebox.Value = (bool)value;
    //             break;
    //         case "AbsoluteInput":
    //             Instance.Settings.AbsoluteInput.Value = (bool)value;
    //             break;
    //         case "RecordReplays":
    //             Instance.Settings.RecordReplays.Value = (bool)value;
    //             break;
    //         case "HitPopups":
    //             Instance.Settings.HitPopups.Value = (bool)value;
    //             break;
    //         case "MissPopups":
    //             Instance.Settings.MissPopups.Value = (bool)value;
    //             break;
    //         case "FPS":
    //             Instance.Settings.FPS.Value = (int)value;
    //             break;
    //         case "UnlockFPS":
    //             Instance.Settings.UnlockFPS.Value = (bool)value;
    //             break;
    //     }
    // }

    public static void Save(string profile = null)
    {
        profile ??= Util.Misc.GetProfile();

        string data = SettingsProfileConverter.Serialize(Instance.Settings);

        File.WriteAllText($"{Constants.USER_FOLDER}/profiles/{profile}.json", data);

        Logger.Log($"Saved settings {profile}");

        SkinManager.Save();
    }

    public static void Load(string profile = null)
    {
        profile ??= Util.Misc.GetProfile();

        Exception err = null;

        try
        {
            SettingsProfileConverter.Deserialize($"{Constants.USER_FOLDER}/profiles/{profile}.json", Instance.Settings);

            ToastNotification.Notify($"Loaded profile [{profile}]");
        }
        catch (Exception exception)
        {
            err = exception;
        }

        if (!Directory.Exists($"{Constants.USER_FOLDER}/skins/{Instance.Settings.Skin.Value}"))
        {
            Instance.Settings.Skin.Value = new("default");
            ToastNotification.Notify($"Could not find skin {Instance.Settings.Skin.Value}", 1);
        }

        if (err != null)
        {
            ToastNotification.Notify("Settings file corrupted", 2);
            throw Logger.Error($"Settings file corrupted; {err}");
        }

        Logger.Log($"Loaded settings {profile}");

        SkinManager.Load();
    }
}
