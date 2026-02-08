using Godot;
using System;
using System.Collections.Generic;

public partial class PeerPlayer3D : GameEntity
{
    CharacterModel _characterModel;
    MovementLogic _movement;
    Mount _mountInstance;

    public override void _Ready()
    {
        base._Ready();
        _movement = new(this);
        _characterModel = GetNode<CharacterModel>("CharacterModel");
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        var moving = _movement.SimpleMovement(delta);
        if (moving)
        {
            if (_mountInstance == null)
            {
                if(_characterModel.CurrentAnimation != CharacterAnimationType.Walk)
                {
                    _characterModel.PlayAnimation(CharacterAnimationType.Walk);
                }
            }
            else
            {
                _characterModel.PlayAnimation(CharacterAnimationType.RideHorse);
                _mountInstance.PlayAnimation(CharacterAnimationType.RideHorse);
            }
        }
        else
        {
            if(_characterModel.CurrentAnimation != CharacterAnimationType.Idle)
            {
                if(_mountInstance == null)
                {
                    _characterModel.PlayAnimation(CharacterAnimationType.Idle);
                }
                else
                {
                    _characterModel.PlayAnimation(CharacterAnimationType.RideHorse);
                    _mountInstance.PlayAnimation(CharacterAnimationType.Idle);
                }
            }
        }

    }
    public void OnMovementUpdate(Vector3 serverPos, Vector3 moveDir, float moveSpeed, Vector3 lookDir, bool isMoving, long serverTime)
    {
        _movement.MovementUpdate(serverPos, moveDir, moveSpeed, lookDir, isMoving, serverTime);
    }

    public void EquipItem(EquipmentSlot slot, ItemType type)
    {
        if(slot == EquipmentSlot.Unknown || type == ItemType.Unknown) { return; }
        if(slot == EquipmentSlot.Weapon)
        {
            var scenePath = ItemInfo.ItemTypeToScenePath(type, ItemInfo.SceneType.EquipmentScene);
            var scene = ResourceLoader.Load<PackedScene>(scenePath);
            var sceneInstance = scene.Instantiate() as Node3D;
            _characterModel.AttachToWeaponAttachment(sceneInstance);
        }
    }

    public void UnequipItem(EquipmentSlot slot)
    {
        if(slot == EquipmentSlot.Weapon)
        {
            _characterModel.UnattachWeapon();
        }
    }

    public void PlayAnimation(CharacterAnimationType animType)
    {
        _characterModel.PlayAnimation(animType);
    }

    public void MountUp() {
        _mountInstance = Mount.MountTypeToMountSceneDict[MountType.Horse].Instantiate() as Mount;
        AddChild(_mountInstance);
        _mountInstance.Init(_characterModel);
        _mountInstance.Position = Vector3.Zero;

        _characterModel.PlayAnimation(CharacterAnimationType.RideHorse);
    }
    public void MountDown() { 
        if(_mountInstance == null) { return; }
        _mountInstance.QueueFree();
        _mountInstance = null;
    }
}
