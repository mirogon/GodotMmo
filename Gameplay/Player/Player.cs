using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

public partial class Player : CharacterBody3D
{
    public float MovementSpeed = 4.0f;

    Node3D _playerMesh;
    Stopwatch _posUpdateStopwatch = new();
    const float _positionUpdateIntervalMs = 100.0f;
    InventorySystem _inventorySystem;
    HealthSystem _healthSystem;

    CharacterModel _model;
    PlayerUi _ui;

    Character _initializedChar;
    int _currentLevel = 1;
    long _currentExp = 0;

    Enemy _currentEnemyTarget = null;
    PeerPlayer3D _currentPeerPlayerTarget = null;

    PackedScene _targetIndicatorScene = ResourceLoader.Load<PackedScene>("res://Gameplay/TargetIndicator.tscn");
    Node3D _targetIndicator = null;

    PackedScene _npcUiScene = ResourceLoader.Load<PackedScene>("res://Gameplay/Npc/NpcUi/NpcUi.tscn");

    Mount _mountInstance = null;

    QuestUi _questUi;

    CharacterStats _characterStatsUi;

    //Movemen
    const float GRAVITY = 9.81f;
    Vector3 _moveDir = new();

    int _currentAttackCombo = 1;

    public override void _Ready()
    {
        base._Ready();
        _playerMesh = GetNode<Node3D>("Model");

        _ui = GetNode<PlayerUi>("Ui");
        _ui.SetLevelAndExp(1, 0);

        _posUpdateStopwatch.Start();

        _inventorySystem = GetNode("Ui/InventorySystem") as InventorySystem;

        _healthSystem = GetNode("HealthSystem") as HealthSystem;

        _model = GetNode<CharacterModel>("Model");

        _questUi = FindChild("QuestUi") as QuestUi;

        _characterStatsUi = FindChild("CharacterStatsUi") as CharacterStats;

        NetworkClient.KnownItemsUpdate += OnItemsUpdate;
        NetworkClient.CharacterHealthUpdate += OnCharacterHealthUpdate;
        NetworkClient.EquippedItemsUpdate += OnEquippedItemsUpdate;
        NetworkClient.CharacterExpUpdate += OnExpUpdate;
        NetworkClient.DamageHitsUpdate += OnDamageHitsUpdate;
        NetworkClient.QuestsProgressUpdate += OnQuestsProgressUpdate;
        NetworkClient.MountUpdate += OnMountUpdate;
        NetworkClient.KnownCharacterUpdate += OnKnownCharacterUpdate;
        NetworkClient.ItemUpgradeResultUpdate += OnItemUpgradeResultUpdate;

        _model.AnimationEvent += OnModelAnimationEvent;

        OnItemsUpdateDeferred();
        OnEquippedItemsUpdateDeferred();
        OnExpUpdateDeferred(_initializedChar.Level, _initializedChar.Exp, _initializedChar.AvailableStatPoints);
        OnQuestsProgressUpdateDeferred();
        OnKnownCharacterUpdateDeferred();
    }

    private void OnModelAnimationEvent(string eventName)
    {
        if(eventName.Contains("Attack") && eventName.Length == 7)
        {
            OnAttackAnimationEvent();
        }

        bool spaceIsDown = Input.IsPhysicalKeyPressed(Key.Space);
        if (eventName.Contains("AttackEnd"))
        {
            if (spaceIsDown)
            {
                WeaponAttack();
            }

        }
        
        if(eventName == "IdleStart")
        {
            _currentAttackCombo = 1;
        }

        switch (eventName)
        {
        }
    }

    public void Initialize(Character c)
    {
        Position = new (c.PositionOnMap.X, c.PositionOnMap.Y, c.PositionOnMap.Z);
        _initializedChar = c;
    }

    public bool MouseIsBlockedByUi()
    {
        return _inventorySystem.MouseIsBlockedByUi();
    }

    void OnItemsUpdate()
    {
        CallDeferred("OnItemsUpdateDeferred");
    }

    void OnItemsUpdateDeferred()
    {
        //if(NetworkClient.KnownInventoryItems.Count <= 0) { return; }
        _inventorySystem.HandleInventoryUpdate(NetworkClient.KnownInventoryItems, NetworkClient.KnownCurrencyAmount);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (Input.IsActionJustPressed("PickUp"))
        {
            PickUpItem();
        }

        if (Input.IsActionJustPressed("WeaponAttack"))
        {
            WeaponAttack();
        }

        if (Input.IsActionJustPressed("EquipDebug"))
        {
            TestEquipItem();
        }

        if (Input.IsActionJustPressed("Logout"))
        {
            Logout();
        }

        if (Input.IsActionJustPressed("RightClick"))
        {
            HandleRightClick();
        }
        if (Input.IsActionJustPressed("ToggleInventory"))
        {
            _inventorySystem.ToggleInventory();
        }
        if (Input.IsActionJustPressed("Mount"))
        {
            HandleMount();
            GD.Print("Floor: " + Utility.GetFloorHeight(GlobalPosition.X, GlobalPosition.Z, GetWorld3D()));
        }
    }

    void HandleMovement(double delta)
    {
        _moveDir = Vector3.Zero;
        if (Input.IsPhysicalKeyPressed(Key.W))
        {
            _moveDir += -_playerMesh.GlobalTransform.Basis.Z;
        }
        if (Input.IsPhysicalKeyPressed(Key.S))
        {
            _moveDir += _playerMesh.GlobalTransform.Basis.Z;
        }
        if (Input.IsPhysicalKeyPressed(Key.A))
        {
            _moveDir += -_playerMesh.GlobalTransform.Basis.X;
        }
        if (Input.IsPhysicalKeyPressed(Key.D))
        {
            _moveDir += _playerMesh.GlobalTransform.Basis.X;
        }

        _moveDir = _moveDir.Normalized();

        _moveDir *= MovementSpeed;
        var velocity = new Vector3(_moveDir.X, Velocity.Y, _moveDir.Z);
        velocity.Y -= GRAVITY * (float)delta;

        //Position += _moveDir * MovementSpeed * (float)delta;

        Velocity = velocity;

        if(_posUpdateStopwatch.ElapsedMilliseconds >= _positionUpdateIntervalMs && NetworkClient.SuccessfullyLoggedIn)
        {
            SendPositionUpdate();
            _posUpdateStopwatch.Restart();
        }
    }
    void HandleMovementAnimations()
    {
        if(_moveDir.Length() > 0.05f && _model.CurrentAnimation != CharacterAnimationType.Walk)
        {
            if(_mountInstance == null)
            {
                _model.PlayAnimation(CharacterAnimationType.Walk);
            }
            else
            {
                _mountInstance.PlayAnimation(CharacterAnimationType.RideHorse); 
            }
        }
        else if(_moveDir.Length() <= 0.05f && _model.CurrentAnimation != CharacterAnimationType.Idle)
        {
            if(_mountInstance == null)
            {
                _model.PlayAnimation(CharacterAnimationType.Idle);
            }
            else
            {
                _mountInstance.PlayAnimation(CharacterAnimationType.Idle); 
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        HandleMovement(delta);
        HandleMovementAnimations();

        MoveAndSlide();
    }

    void WeaponAttack()
    {
        CharacterAnimationType type = CharacterAnimationType.Attack1 + (short)(_currentAttackCombo-1);

        _model.PlayAnimation(type);
        NetworkClient.SendCharacterAnimationStart(type);

        ++_currentAttackCombo;
        if(_currentAttackCombo > 4)
        {
            _currentAttackCombo = 1;
        }
    }


    void HandleRightClick()
    {
        GameEntity ge = Utility.CheckIfMouseOnGameEntity(this);
        if(ge is Npc npc) {
            OpenNpcUi(npc);
            return;
        }
        SetTargetIndicator(ge);
        if(ge is Enemy enemy)
        {
            _currentEnemyTarget = enemy;
            _currentPeerPlayerTarget = null;
        }
        if(ge is PeerPlayer3D peerPlayer)
        {
            _currentPeerPlayerTarget = peerPlayer;
            _currentEnemyTarget = null;
        }
    }
    void OpenNpcUi(Npc npc)
    {
        var npcUiInstance = _npcUiScene.Instantiate() as NpcUi;
        var info = NpcInfo.NpcTypeToNpcInfoDict[npc.NpcType];
        npcUiInstance.Init(info.Name, "", info.AvailableQuests);
        AddChild(npcUiInstance);
    }

    void SetTargetIndicator(Node3D target)
    {
        if(target == null) { return; }
        if(!IsInstanceValid(_targetIndicator))
        {
            _targetIndicator = _targetIndicatorScene.Instantiate() as Node3D;
        }

        if (IsInstanceValid(_currentEnemyTarget))
        {
            _currentEnemyTarget.RemoveChild(_targetIndicator);
        }
        if (IsInstanceValid(_currentPeerPlayerTarget))
        {
            _currentPeerPlayerTarget.RemoveChild(_targetIndicator);
        }

        _targetIndicator.Position = Vector3.Zero;
        target.AddChild(_targetIndicator);
    }

    private void OnAttackAnimationEvent()
    {
        var map = GetParent<MapManager>();
        NetworkClient.WeaponAttack();
    }

    void PickUpItem()
    {
        MapManager currentMapManager = FindParent("MapManager") as MapManager;
        if(currentMapManager.KnownItemsOnMap.Count < 1) { return; }

        var closestItem = currentMapManager.KnownItemsOnMap[0];
        var closestItemPos = Utility.PositionToVector3(closestItem.PositionOnMap);
        for(int i = 0; i < currentMapManager.KnownItemsOnMap.Count; ++i)
        {
            var current = currentMapManager.KnownItemsOnMap[i];
            Vector3 itemPos = Utility.PositionToVector3(current.PositionOnMap);

            if(GlobalPosition.DistanceTo(itemPos) < GlobalPosition.DistanceTo(closestItemPos))
            {
                closestItem = current;
                closestItemPos = itemPos;
            }

        }
        NetworkClient.PickUpItem(closestItem.Id);
    }
    void HandleMount()
    {
        NetworkClient.MountUp(MountType.Horse);
    }

    void TestEquipItem()
    {
        var equippedWeapon = _inventorySystem.EquippedItems[EquipmentSlot.Weapon];
        if (equippedWeapon.Id != Guid.Empty)
        {
            NetworkClient.EquipOrUnequipItem(equippedWeapon.Id, false);
            GD.Print("UNEQUIP WEAPON");
            return;
        }

        foreach(var i in _inventorySystem.Items)
        {
            if(i.Value.ItemType == ItemType.Sword)
            {
                NetworkClient.EquipOrUnequipItem(i.Value.Id, true);
                GD.Print("EQUIP WEAPON");
            }
        }
    }

    void SendPositionUpdate()
    {
        var yDegrees = Mathf.RadToDeg(_playerMesh.Rotation.Y);
        //GD.Print("YROT: " + yDegrees);
        CS_PositionUpdatePacket posUpdate = new(LoginClient.NewestSessionId, Position.X, Position.Y, Position.Z, yDegrees);
        NetworkClient.ReliableUnorderedPacketsToSend.Enqueue((posUpdate, typeof(CS_PositionUpdatePacket)));
    }
    void OnCharacterHealthUpdate((ulong publicId, int currentHealth, int maxHealth) updateInfo)
    {
        _healthSystem.MaxHealth = updateInfo.maxHealth;
        _healthSystem.CurrentHealth = updateInfo.currentHealth;
    }

    void OnEquippedItemsUpdate(UInt64 publicId)
    {
        if(publicId != NetworkClient.PublicId) {
            return;
        }

        CallDeferred("OnEquippedItemsUpdateDeferred");
    }

    void OnEquippedItemsUpdateDeferred()
    {
        if (!NetworkClient.KnownEquippedItems.ContainsKey(NetworkClient.PublicId)) { return; }

        var knownEquippedItems = NetworkClient.KnownEquippedItems[NetworkClient.PublicId];
        _inventorySystem.HandleEquipmentSystemUpdate(knownEquippedItems);

        _model.UnattachWeapon();
        if(knownEquippedItems[EquipmentSlot.Weapon].Id != Guid.Empty)
        {
            //Model
            var scenePath = ItemInfo.ItemTypeToScenePath(knownEquippedItems[EquipmentSlot.Weapon].ItemType, ItemInfo.SceneType.EquipmentScene);
            var scene = ResourceLoader.Load<PackedScene>(scenePath);
            var sceneInstance = scene.Instantiate() as Node3D;
            _model.AttachToWeaponAttachment(sceneInstance);
        }
    }

    void OnExpUpdate()
    {
        CallDeferred("OnExpUpdateDeferred", NetworkClient.KnownCharacterExp.lvl, NetworkClient.KnownCharacterExp.exp, NetworkClient.KnownCharacterExp.availablePoints);
    }

    void OnExpUpdateDeferred(int lvl, long exp, short availablePoints)
    {
        _currentLevel = lvl;
        _currentExp = exp;

        _ui.SetLevelAndExp(_currentLevel, _currentExp);
        _characterStatsUi.Update(availablePoints);
    }
    void OnDamageHitsUpdate()
    {
        CallDeferred("OnDamageHitsUpdateDeferred");
    }

    void OnDamageHitsUpdateDeferred()
    {
        if(NetworkClient.DamageHitsUpdates.Count < 1) { return; }

        var update = NetworkClient.DamageHitsUpdates[0];
        NetworkClient.DamageHitsUpdates.RemoveAt(0);    

        var scene = ResourceLoader.Load<PackedScene>("res://Gameplay/DamageNumber.tscn");
        var instance = scene.Instantiate() as DamageNumber;
        instance.Init(update.Hits[0].Damage);

        var right = _model.Transform.Basis.X;

        instance.Position = right;
        AddChild(instance);
    }


    void Logout()
    {
        NetworkClient.Logout();
    }
    void OnQuestsProgressUpdate()
    {
        CallDeferred("OnQuestsProgressUpdateDeferred");
    }
    void OnQuestsProgressUpdateDeferred()
    {
        for(int i = 0; i < 999; ++i)
        {
            SC_QuestsUpdatePacket current;
            if(!NetworkClient.QuestProgressUpdates.TryTake(out current)) { return; }

            for (int j = 0; j < current.QuestUpdates.Length; ++j)
            {
                var currentQuestUpdate = current.QuestUpdates[j];
                if(currentQuestUpdate.Id == QuestId.Unknown) { continue; }
                _questUi.AddOrUpdateQuest(currentQuestUpdate);
            }
        }
    }
    void OnMountUpdate()
    {
        CallDeferred("OnMountUpdateDeferred");
    }

    void OnMountUpdateDeferred()
    {
        if(NetworkClient.NewestMountUpdate == null) { return; }
        SC_Mount update = NetworkClient.NewestMountUpdate;
        if(update.PublicId != NetworkClient.PublicId) { return; }

        if (update.MountingUp)
        {
            GD.Print("Mounting up");
            var mountInstance = Mount.MountTypeToMountSceneDict[MountType.Horse].Instantiate() as Mount;

            AddChild(mountInstance);
            mountInstance.Position = Vector3.Zero;
            _mountInstance = mountInstance;
            _mountInstance.Init(_model);

            _model.PlayAnimation(CharacterAnimationType.RideHorse);
        }
        else
        {
            GD.Print("Mounting down");
            _mountInstance.QueueFree();
            _mountInstance = null;
        }
    }
    void OnKnownCharacterUpdate()
    {
        CallDeferred("OnKnownCharacterUpdateDeferred");
    }
    void OnKnownCharacterUpdateDeferred()
    {
        var u = NetworkClient.NewestKnownCharacter;
        _characterStatsUi.Update(u.Vitality, u.Intelligence, u.Strength, u.Dexterity, u.AvailableStatPoints);
    }
    void OnItemUpgradeResultUpdate()
    {
        CallDeferred("OnItemUpgradeResultUpdateDeferred");
    }
    void OnItemUpgradeResultUpdateDeferred()
    {
        for(int i= 0; i < 100; ++i)
        {
            SC_ItemUpgradeResultPacket current;
            if(!NetworkClient.ItemUpgradeResultUpdates.TryTake(out current)) { return; }

            if (current.Success)
            {
                var item = new MongoInventoryItem(current.ResultingItem);
                _inventorySystem.UpdateItem(item);
            }
            else
            {
                _inventorySystem.RemoveItem(current.ResultingItem.Id);
                GD.Print("Item Upgrade Failure");
            }
        }
    }
}
