using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MapManager : Node
{
    public static Dictionary<MapType, PackedScene> MapScenes = new Dictionary<MapType, PackedScene>
    {
        {MapType.Map1, ResourceLoader.Load<PackedScene>("res://Gameplay/Maps/Map1.tscn")},
        {MapType.Map2, ResourceLoader.Load<PackedScene>("res://Gameplay/Maps/Map2.tscn")},
    };

    public Player Player;
    public PackedScene PeerPlayerScene = ResourceLoader.Load<PackedScene>("res://Gameplay/Player/PeerPlayer.tscn");

    public static PackedScene InfoWindowScreen = ResourceLoader.Load<PackedScene>("res://Gameplay/InfoWindow.tscn");
    public static PackedScene SelectCharacterScene = ResourceLoader.Load<PackedScene>("res://Gameplay/CharacterScreen/SelectCharacterScene.tscn");

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
        NetworkClient.EquippedItemsUpdate += OnEquippedItemsUpdate;
        NetworkClient.CharacterAnimationUpdate += OnCharacterAnimationUpdate;
        NetworkClient.CharacterLoggedOut += OnCharacterLoggedOut;
        NetworkClient.MonsterAnimationUpdate += OnMonsterAnimationUpdate;
    }


    public void Initialize(Character c)
    {
        Player = GetNode<Player>("Player");
        Player.Initialize(c);
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

            if (NetworkClient.KnownEquippedItems.ContainsKey(update.PublicId))
            {
                OnEquippedItemsUpdateDeferred(update.PublicId);
            }

            return;
        }

        var currentInstance = _peerInstances[update.PublicId];
        currentInstance.OnMovementUpdate(update.Position.ToVector3(), update.MoveDir.ToVector3(), update.MoveSpeed,  update.YRotationEuler, update.IsMoving, update.ServerTimeUtcUnixMs);
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
            var itemScene = ResourceLoader.Load<PackedScene>(ItemInfo.ItemTypeToScenePath(current.ItemType, ItemInfo.SceneType.GroundScene));
            ItemOnGround itemInstance = itemScene.Instantiate() as ItemOnGround;
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
            ItemOnGround current = (ItemOnGround)itemInstances[i];
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
            //e.Value.QueueFree();
        }

        //EnemyInstances.Clear();

        for(int i = 0; i < 99999; ++i)
        {
            EnemyData c;
            if(!NetworkClient.NewestEnemiesOnMapUpdate.TryTake(out c)) {
                GD.Print("NewestEnemyUpdate trytake");
                return; 
            }

            GD.Print("Took Enemy out of update");
            if(c.EnemyType == EEnemyType.Unknown) { 
                GD.Print("Skipping unknown enemy type");
                continue; 
            }

            var enemyScenePath = Classes.EnemyTypeToEnemySceneDictionary[c.EnemyType];
            var enemyScene = GD.Load<PackedScene>(enemyScenePath);
            Enemy enemyInstance = enemyScene.Instantiate() as Enemy;
            enemyInstance.Init(c.Id, c.CurrentHealth, c.MaxHealth);

            EnemyInstances.Add(c.Id, enemyInstance);
            GetNode("Enemies").AddChild(enemyInstance);
            GD.Print("Add enemy as child");
            enemyInstance.Position = c.PositionOnMap.ToVector3();
        }
    }
    private void OnMonsterPositionUpdate()
    {
        CallDeferred("OnMonsterPositionUpdateDeferred");
    }
    void OnMonsterPositionUpdateDeferred()
    {
        for(int i = 0; i < 99999; ++i)
        {
            SC_MonsterPositionUpdatePacket newUpdate;
            if(!NetworkClient.MonsterPosUpdates.TryTake(out newUpdate)) { return; }
            if (!EnemyInstances.ContainsKey(newUpdate.Id)) { continue; }

            var instance = EnemyInstances[newUpdate.Id];
            instance.MovementUpdate(newUpdate.Position.ToVector3(), newUpdate.Velocity.ToVector3().Normalized(), newUpdate.Velocity.ToVector3().Length(), 0, newUpdate.IsMoving, newUpdate.ServerTimeUtcUnixMs);

            Vector3 lookAtPos = instance.GlobalPosition + newUpdate.Velocity.ToVector3().Normalized();
            instance.LookAt(lookAtPos);
        }
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
            if(enemy.HealthSystem.IsDead)
            {
                EnemyInstances.Remove(c.Id);
            }
        }
    }

    void OnEquippedItemsUpdate(ulong publicId)
    {
        CallDeferred("OnEquippedItemsUpdateDeferred", publicId);
    }

    void OnEquippedItemsUpdateDeferred(ulong publicId)
    {
        if (!_peerInstances.ContainsKey(publicId))
        {
            GD.Print("Peer instance with public id not found: " +  publicId);
            return;
        }

        var peerInstance = _peerInstances[publicId];

        var update = NetworkClient.KnownEquippedItems[publicId];
        var weapon = update[EquipmentSlot.Weapon];

        peerInstance.UnequipItem(EquipmentSlot.Weapon);
        peerInstance.EquipItem(EquipmentSlot.Weapon, weapon.ItemType);
    }

    void OnCharacterAnimationUpdate(ulong publicId, short animationType)
    {
        CallDeferred("OnCharacterAnimationUpdateDeferred", publicId, animationType);
    }

    void OnCharacterAnimationUpdateDeferred(ulong publicId, short animationType)
    {
        if (!_peerInstances.ContainsKey(publicId)) { return; }
        var peer = _peerInstances[publicId];

        peer.PlayAnimation((CharacterAnimationType)animationType);
    }

    void OnCharacterLoggedOut(ulong publicId)
    {
        if (!_peerInstances.ContainsKey(publicId)) { return; }

        var peer = _peerInstances[publicId];
        _peerInstances.Remove(publicId);
        peer.QueueFree();
    }
    void OnMonsterAnimationUpdate()
    {
        CallDeferred("OnMonsterAnimationUpdateDeferred");
    }

    void OnMonsterAnimationUpdateDeferred()
    {
        for(int i = 0; i < NetworkClient.MonsterAnimationUpdates.Count; ++i)
        {
            var update = NetworkClient.MonsterAnimationUpdates[0];
            NetworkClient.MonsterAnimationUpdates.RemoveAt(0);

            if (!EnemyInstances.ContainsKey(update.MonsterId)) { continue; }
            var enemyInstance = EnemyInstances[update.MonsterId];
            enemyInstance.PlayAnimation(update.AnimationType);
        }
    }
}

