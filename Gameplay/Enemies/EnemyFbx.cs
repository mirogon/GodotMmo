using Godot;
using System;

public partial class EnemyFbx : Node3D
{
    AnimationTree _animationTree;
    public override void _Ready()
    {
        base._Ready();
        _animationTree = GetNode<AnimationTree>("AnimationTree");
    }

    public void PlayAnimation(MonsterAnimationType animType)
    {
        switch (animType)
        {
            case MonsterAnimationType.Idle: _animationTree.Set("parameters/IdleMoveBlend/blend_amount", 0.0f); break;
            case MonsterAnimationType.Walk:
                _animationTree.Set("parameters/WalkRunBlend/blend_amount", 0.0f);
                _animationTree.Set("parameters/IdleMoveBlend/blend_amount", 1.0f);
                break;
            case MonsterAnimationType.Run: 
                _animationTree.Set("parameters/WalkRunBlend/blend_amount", 1.0f);
                _animationTree.Set("parameters/IdleMoveBlend/blend_amount", 1.0f);
                break;
            case MonsterAnimationType.Attack: _animationTree.Set("parameters/AttackOneShot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire); break;
        }
    }
}
