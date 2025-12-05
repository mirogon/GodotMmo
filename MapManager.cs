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

    Dictionary<UInt64, PeerPlayer3D> _peerInstances = new();

    public List<MongoMapItem> KnownItemsOnMap = new();

    public Dictionary<Guid, Enemy> EnemyInstances = new();

    public override void _Ready()
    {
        Player = GetNode<Player>("Player");
        NetworkClient.PeerPlayerPositionUpdate += OnPeerPlayerPositionUpdate;
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

    void OnPeerPlayerPositionUpdate()
    {
        CallDeferred("OnPeerPlayerPositionUpdateDeferred");
    }

    void OnPeerPlayerPositionUpdateDeferred()
    {
        if (PeerPlayerScene == null)
        {
            PeerPlayerScene = ResourceLoader.Load<PackedScene>("res://Gameplay/Player/PeerPlayer.tscn");
        }

        SC_PeerPlayerPositionUpdatePacket update = new();
        if(!NetworkClient.PeerPlayerPositionUpdateQueue.TryDequeue(out update)) { return; }

        if (!_peerInstances.ContainsKey(update.PublicId))
        {
            GD.Print("PeerPlayerScene: ", PeerPlayerScene);
            var instance = PeerPlayerScene.Instantiate() as PeerPlayer3D;
            AddChild(instance);
            var ppInstance = instance as PeerPlayer3D;
            _peerInstances.Add(update.PublicId, ppInstance);
            ppInstance.Position = update.Position.ToVector3();
            var rot = ppInstance.Rotation;
            rot.Y = Mathf.DegToRad(update.YRotationEuler);
            ppInstance.Rotation = rot;
            return;
        }

        var currentInstance = _peerInstances[update.PublicId];
        currentInstance.OnMovementUpdate(update.Position.ToVector3(), update.MoveDir.ToVector3(), update.MoveSpeed, update.IsMoving, update.ServerTimeUtcUnixMs);
        var r = currentInstance.Rotation;
        r.Y = Mathf.DegToRad(update.YRotationEuler);
        currentInstance.Rotation = r;

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
        instance.MovementUpdate(newUpdate.Position.ToVector3(), newUpdate.Velocity.ToVector3().Normalized(), newUpdate.Velocity.ToVector3().Length(), newUpdate.IsMoving, newUpdate.ServerTimeUtcUnixMs);
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

