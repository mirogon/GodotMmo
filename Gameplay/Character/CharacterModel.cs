using Godot;
using System;

public enum CharacterAnimationType
{
    Unknown = 0,
    Idle = 1,
    Walk = 2,
    Attack1
}
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

        TestAttachKatana();
    }

    void TestAttachKatana()
    {
        var katanaScene = ResourceLoader.Load<PackedScene>("Gameplay/Weapons/Katana.tscn");
        var katanaInstance = katanaScene.Instantiate() as Node3D;
        AttachToWeaponAttachment(katanaInstance);
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
        else if(animType == CharacterAnimationType.Attack1)
        {
            _animationTree.Set("parameters/Attack/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
        }
    }
}
