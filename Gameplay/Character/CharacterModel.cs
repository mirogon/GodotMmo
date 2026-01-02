using Godot;
using System;

public enum CharacterAnimationType
{
    Unknown = 0,
    Idle = 1,
    Walk = 2
}
public partial class CharacterModel : Node3D
{
    public CharacterAnimationType CurrentAnimation { get; private set; }
    AnimationTree _animationTree;
    public override void _Ready()
    {
        base._Ready();
        _animationTree = GetNode<AnimationTree>("AnimationTree");
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
    }
}
