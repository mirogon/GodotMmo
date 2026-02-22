using Godot;
using System;
using System.Net.Http.Headers;

public partial class CharacterModel : Node3D
{
    public Action<string> AnimationEvent;
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
            _animationTree.Set("parameters/Transition/transition_request", "Idle");
        }
        else if (animType == CharacterAnimationType.Walk)
        {
            _animationTree.Set("parameters/Transition/transition_request", "Run");
        }
        else if(animType == CharacterAnimationType.RideHorse)
        {
            _animationTree.Set("parameters/Transition/transition_request", "RideHorse");
        }
        else if(animType == CharacterAnimationType.HorseAttack)
        {
            _animationTree.Set("parameters/Transition/transition_request", "HorseAttack");
        }

        else if (animType == CharacterAnimationType.Attack1)
        {
            _animationTree.Set("parameters/Attack1/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
            _animationTree.Set("parameters/Attack2/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);
            _animationTree.Set("parameters/Attack3/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);
            _animationTree.Set("parameters/Attack4/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);
        }
        else if (animType == CharacterAnimationType.Attack2)
        {

            _animationTree.Set("parameters/Attack1/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);
            _animationTree.Set("parameters/Attack2/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
            _animationTree.Set("parameters/Attack3/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);
            _animationTree.Set("parameters/Attack4/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);
        }
        else if (animType == CharacterAnimationType.Attack3)
        {
            _animationTree.Set("parameters/Attack1/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);
            _animationTree.Set("parameters/Attack2/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);
            _animationTree.Set("parameters/Attack3/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
            _animationTree.Set("parameters/Attack4/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);
        }
        else if (animType == CharacterAnimationType.Attack4)
        {
            _animationTree.Set("parameters/Attack1/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);
            _animationTree.Set("parameters/Attack2/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);
            _animationTree.Set("parameters/Attack3/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);
            _animationTree.Set("parameters/Attack4/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);

        }
    }
    public void HandleAnimationEvent(string eventName)
    {
        AnimationEvent?.Invoke(eventName);
    }
}
