using Godot;
using System;

public partial class Cursor : TextureRect, ISkinnable
{
    public override void _Ready()
    {
        SkinManager.Instance.Loaded += UpdateSkin;
        // SettingsManager.Instance.Settings.FieldUpdated += (field, value) => { if (field == "CursorScale") { UpdateSize(); } };

        UpdateSkin();
        UpdateSize();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
		{
            Position = mouseMotion.Position - Size / 2;
        }
    }

	public void UpdateSize()
	{
        Size = Vector2.One * 32 * (float)SettingsManager.Instance.Settings.CursorScale;
    }

	public void UpdateSkin(SkinProfile skin = null)
    {
        skin ??= SkinManager.Instance.Skin;

        Texture = skin.CursorImage;
    }
}