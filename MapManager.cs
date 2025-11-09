using Godot;
using System;
using System.Collections.Generic;

public partial class MapManager : Node
{
    public static Dictionary<MapType, PackedScene> MapScenes = new Dictionary<MapType, PackedScene>
    {
        {MapType.Map1, ResourceLoader.Load<PackedScene>("res://Scenes/Maps/Map1.tscn")},
        {MapType.Map2, ResourceLoader.Load<PackedScene>("res://Scenes/Maps/Map2.tscn")},
    };

    [Export] public Node3D Player;
    [Export] public PackedScene PeerPlayerScene;

    public static PackedScene InfoWindowScreen = ResourceLoader.Load<PackedScene>("res://Scenes/InfoWindow.tscn");
    public static PackedScene SelectCharacterScene = ResourceLoader.Load<PackedScene>("res://Scenes/SelectCharacterScene.tscn");

    List<Node3D> _peerInstances = new();

    public List<MongoMapItem> KnownItemsOnMap = new();

    public override void _Ready()
    {
        NetworkClient.PlayerUpdate += OnPlayerUpdate;
        NetworkClient.NewItemsOnMapUpdate += OnItemsUpdate;
        NetworkClient.RemovedItemsOnMap += OnItemsRemovedUpdate;
    }


    public void Initialize(Position playerPos)
    {
        Player.Position = new Vector3(playerPos.X, 0, playerPos.Z);
    }

    void OnPlayerUpdate(List<PeerPlayer> peers)
    {
        foreach(var pi in _peerInstances)
        {
            pi.QueueFree();
        }
        _peerInstances.Clear();


        var instance = PeerPlayerScene.Instantiate();
        CallDeferred("add_child", instance);
        //GetParent().AddChild(instance);
        var n3d = instance as Node3D;
        _peerInstances.Add(n3d);
        n3d.Position = peers[0].Position;
        var rot = n3d.Rotation;
        rot.Y = Mathf.DegToRad(peers[0].YRotationEuler);
        n3d.Rotation = rot;
        GD.Print("OnPlayerUpdate PeerPos: X:" + n3d.Position.X +  " Y:" + n3d.Position.Y + " Z:" + n3d.Position.Z);
    }
    void OnItemsUpdate(List<MongoMapItem> list)
    {
        KnownItemsOnMap.AddRange(list);
        for (int i = 0; i < list.Count; i++)
        {
            var current = list[i];
            if(current.ItemType == ItemType.Unknown) { continue; }
            var itemScene = ItemManager.ItemMeshScenes[current.ItemType];
            ItemInstance itemInstance = itemScene.Instantiate() as ItemInstance;
            if(itemInstance == null) { continue; }
            itemInstance.ItemId = current.Id;
            itemInstance.Position = new Vector3(current.PositionOnMap.X, 0, current.PositionOnMap.Z);
            GetNode("Items").CallDeferred("add_child", itemInstance);
        }
    }
    void OnItemsRemovedUpdate(List<Guid> list)
    {
        var itemInstances = GetNode("Items").GetChildren();

        var toDelete = new List<MongoMapItem>();
        for (int i = 0; i < KnownItemsOnMap.Count; ++i){
            var current = KnownItemsOnMap[i];
            if (list.Contains(current.Id))
            {
                toDelete.Add(current);
            }
        }
        foreach(var del in toDelete)
        {
            KnownItemsOnMap.Remove(del);
        }

        for (int i = 0; i < itemInstances.Count; i++)
        {
            ItemInstance current = (ItemInstance)itemInstances[i];
            if (list.Contains(current.ItemId))
            {
                current.QueueFree();
            }
        }
    }
}
