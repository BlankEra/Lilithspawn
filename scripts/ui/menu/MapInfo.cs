using Godot;
using System;

public partial class MapInfo : AspectRatioContainer
{
    public Map SelectedMap;

    private MapList mapList;
    private Panel holder;

    private readonly PackedScene infoContainerTemplate = ResourceLoader.Load<PackedScene>("res://prefabs/map_info_container.tscn");
    private MapInfoContainer infoContainer;

    public override void _Ready()
    {
        mapList = GetParent().GetNode<MapList>("MapList");
		
        mapList.OnMapSelected += Select;

        holder = GetNode<Panel>("Holder");
    }
	
    public override void _Draw()
    {
        float height = (AnchorBottom - AnchorTop) * GetParent<Control>().Size.Y - OffsetTop + OffsetBottom;

        holder.CustomMinimumSize = Vector2.One * Math.Min(850, height);
    }

	public void Select(Map map)
	{
        if (map == SelectedMap) { return; }

        SelectedMap = map;
		
        var oldContainer = infoContainer;
        infoContainer?.Transition(false).TweenCallback(Callable.From(() => { holder.RemoveChild(oldContainer); oldContainer.QueueFree(); }));

        infoContainer = infoContainerTemplate.Instantiate<MapInfoContainer>();

        holder.AddChild(infoContainer);
		infoContainer.Setup(map);
        infoContainer.Transition(true);
    }
}