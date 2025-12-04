using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MapManager : Node
{
    public static Dictionary<MapType, PackedScene> MapScenes = new Dictionary<MapType, PackedScene>
    {
        {MapType.Map1, ResourceLoader.Load<PackedScene>("res://Scenes/Maps/Map1.tscn")},
        {MapType.Map2, ResourceLoader.Load<PackedScene>("res://Scenes/Maps/Map2.tscn")},
    };

    public Player Player;
    public PackedScene PeerPlayerScene = ResourceLoader.Load<PackedScene>("res://Gameplay/Player/PeerPlayer.tscn");

    public static PackedScene InfoWindowScreen = ResourceLoader.Load<PackedScene>("res://Scenes/InfoWindow.tscn");
    public static PackedScene SelectCharacterScene = ResourceLoader.Load<PackedScene>("res://Scenes/SelectCharacterScene.tscn");

    List<Node3D> _peerInstances = new();

    public List<MongoMapItem> KnownItemsOnMap = new();

    public Dictionary<Guid, Enemy> EnemyInstances = new();

    public override void _Ready()
    {
        Player = GetNode<Player>("Player");
        NetworkClient.PlayerUpdate += OnPlayerUpdate;
        NetworkClient.NewItemsOnMapUpdate += OnItemsUpdate;
        NetworkClient.RemovedItemsOnMap += OnItemsRemovedUpdate;
        NetworkClient.EnemiesOnMapUpdate += OnEnemiesOnMapUpdate;
        NetworkClient.MonsterPositionUpdate += OnMonsterPositionUpdate;
        NetworkClient.MonstersHealthUpdate += OnMonstersHealthUpdate;
    }


    public void Initialize(int maxHealth, int currentHealth, bool isDead, M1Vector3 playerPos)
    {
        Player = GetNode<Player>("Player");
        Player.Initialize(maxHealth, currentHealth, isDead, playerPos);
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
    void OnEnemiesOnMapUpdate()
    {
        GD.Print("ENEMY UPDATE");
        CallDeferred("OnEnemiesUpdateDeferred");
    }

    void OnEnemiesUpdateDeferred()
    {
        foreach(var e in EnemyInstances)
        {
            e.Value.QueueFree();
        }

        EnemyInstances.Clear();


        foreach(var c in NetworkClient.NewestEnemiesOnMapUpdate)
        {
            if(c.EnemyType == EEnemyType.Unknown) { continue; }

            var enemyScenePath = Classes.EnemyTypeToEnemySceneDictionary[c.EnemyType];
            var enemyScene = GD.Load<PackedScene>(enemyScenePath);
            Enemy enemyInstance = enemyScene.Instantiate() as Enemy;
            EnemyInstances.Add(c.Id, enemyInstance);
            GetNode("Enemies").CallDeferred("add_child", enemyInstance);
            enemyInstance.Position = c.PositionOnMap.ToVector3();
        }
    }
    private void OnMonsterPositionUpdate()
    {
        CallDeferred("OnMonsterPositionUpdateDeferred");
    }
    void OnMonsterPositionUpdateDeferred()
    {
        var newUpdate = NetworkClient.NewerstMonsterPosUpdate;

        if (!EnemyInstances.ContainsKey(newUpdate.Id)) { return; }

        var instance = EnemyInstances[newUpdate.Id];
        instance.MovementUpdate(newUpdate.Position.ToVector3(), newUpdate.Velocity.ToVector3(), newUpdate.IsMoving, newUpdate.ServerTimeUtcUnixMs);
    }
    void OnMonstersHealthUpdate()
    {
        SC_MonstersHealthUpdate update;
        NetworkClient.MonstersHealthUpdateQueue.TryDequeue(out update);

        for(int i = 0; i < update.HealthUpdates.Length; ++i)
        {
            var c = update.HealthUpdates[i];
            if (!EnemyInstances.ContainsKey(c.Id)) { continue; }

            var enemy = EnemyInstances[c.Id];
            enemy.HealthSystem.CurrentHealth = c.CurrentHealth;
        }
    }

}

