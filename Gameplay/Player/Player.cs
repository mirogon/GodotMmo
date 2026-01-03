using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class Player : Node3D
{
    public float MovementSpeed = 4.0f;

    Node3D _playerMesh;
    Stopwatch _posUpdateStopwatch = new();
    const float _positionUpdateIntervalMs = 100.0f;
    InventorySystem _inventorySystem;
    HealthSystem _healthSystem;

    CharacterModel _model;

    public override void _Ready()
    {
        base._Ready();
        _playerMesh = GetNode<Node3D>("Model");
        _posUpdateStopwatch.Start();

        _inventorySystem = GetNode("InventorySystem") as InventorySystem;

        _healthSystem = GetNode("HealthSystem") as HealthSystem;

        _model = GetNode<CharacterModel>("Model");

        NetworkClient.KnownItemsUpdate += OnItemsUpdate;
        NetworkClient.CharacterHealthUpdate += OnCharacterHealthUpdate;
    }


    public void Initialize(int maxHealth, int currentHealth, bool isDead, M1Vector3 pos)
    {
        Position = new (pos.X, pos.Y, pos.Z);    
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
        _inventorySystem.HandleInventoryUpdate(NetworkClient.KnownInventoryItems);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        Vector3 moveDir = Vector3.Zero;
        if (Input.IsPhysicalKeyPressed(Key.W))
        {
            moveDir += -_playerMesh.GlobalTransform.Basis.Z;
        }
        if (Input.IsPhysicalKeyPressed(Key.S))
        {
            moveDir += _playerMesh.GlobalTransform.Basis.Z;
        }
        if (Input.IsPhysicalKeyPressed(Key.A))
        {
            moveDir += -_playerMesh.GlobalTransform.Basis.X;
        }
        if (Input.IsPhysicalKeyPressed(Key.D))
        {
            moveDir += _playerMesh.GlobalTransform.Basis.X;
        }
        moveDir = moveDir.Normalized();

        if(moveDir.Length() > 0.05f && _model.CurrentAnimation != CharacterAnimationType.Walk)
        {
            _model.PlayAnimation(CharacterAnimationType.Walk);
        }
        else if(moveDir.Length() <= 0.05f && _model.CurrentAnimation != CharacterAnimationType.Idle)
        {
            _model.PlayAnimation(CharacterAnimationType.Idle);
        }

        Position += moveDir * MovementSpeed * (float)delta;
        if(_posUpdateStopwatch.ElapsedMilliseconds >= _positionUpdateIntervalMs && NetworkClient.SuccessfullyLoggedIn)
        {
            SendPositionUpdate();
            _posUpdateStopwatch.Restart();
        }

        if (Input.IsActionJustPressed("PickUp"))
        {
            PickUpItem();
        }

        if (Input.IsActionJustPressed("WeaponAttack"))
        {
            WeaponAttack();
        }
    }
    void WeaponAttack()
    {
        var map = GetParent<MapManager>();

        List<Guid> toAttack = new();
        foreach(var enemy in map.EnemyInstances)
        {
            var enemyValue = enemy.Value;
            toAttack.Add(enemy.Key);
        }

        NetworkClient.WeaponAttackMonsters(toAttack);
        _model.PlayAnimation(CharacterAnimationType.Attack1);
    }

    void PickUpItem()
    {
        MapManager currentMapManager = FindParent("MapManager") as MapManager;

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
}
