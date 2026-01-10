using Godot;
using System;

public partial class CharacterModel : Node3D
{
    public CharacterAnimationType CurrentAnimation { get; private set; }
    AnimationTree _animationTree;
    Node3D _weaponAttachment;
    public override void _Ready()
    {
        base._Ready();
        _animationTree = GetNode<AnimationTree>("AnimationTree");
        _weaponAttachment = FindChild("WeaponAttachment") as Node3D;

        //TestAttachKatana();
    }

    void TestAttachKatana()
    {
        var katanaScene = ResourceLoader.Load<PackedScene>("Gameplay/Items/Weapons/Sword_Eq.tscn");
        var katanaInstance = katanaScene.Instantiate() as Node3D;
        AttachToWeaponAttachment(katanaInstance);
    }

    public void UnattachWeapon()
    {
        if(_weaponAttachment.GetChildCount() <= 0) { return; }
        _weaponAttachment.GetChild(0).QueueFree();
    }
    public void AttachToWeaponAttachment(Node3D obj)
    {
        obj.Position = Vector3.Zero;
        _weaponAttachment.AddChild(obj);
    }
    public void PlayAnimation(CharacterAnimationType animType)
    {
        CurrentAnimation = animType;
        if (animType == CharacterAnimationType.Idle)
        {
            _animationTree.Set("parameters/IdleWalkSpace/blend_position", -1.0f);
        }
        else if (animType == CharacterAnimationType.Walk)
        {
            _animationTree.Set("parameters/IdleWalkSpace/blend_position", 1.0f);
        }
        else if (animType == CharacterAnimationType.WeaponAttack)
        {
            _animationTree.Set("parameters/Attack/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
        }
    }
}
