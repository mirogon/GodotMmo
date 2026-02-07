using Godot;
using Godot.Collections;
using System;

public partial class Mount : GameEntity
{
    CharacterModel _characterModel;
    AnimationTree _animTree;

    public static Dictionary<MountType, PackedScene> MountTypeToMountSceneDict = new()
    {
        {MountType.Horse, ResourceLoader.Load<PackedScene>("res://Gameplay/Mounts/Horse/HorseModel.tscn")}
    };

    public override void _Ready()
    {
        base._Ready();
        _animTree = GetNode("AnimationTree") as AnimationTree;
    }
    public void Init(CharacterModel model)
    {
        _characterModel = model;
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        if(_characterModel == null) { return; }
        if(Quaternion != _characterModel.Quaternion)
        {
            Quaternion = _characterModel.Quaternion;
        }
    }
    public void PlayAnimation(CharacterAnimationType type)
    {
        if(type == CharacterAnimationType.RideHorse)
        {
            _animTree.Set("parameters/IdleRunBlend/blend_amount", 1.0f);
        }
        if(type == CharacterAnimationType.Idle)
        {
            _animTree.Set("parameters/IdleRunBlend/blend_amount", 0.0f);
        }
    }
}
