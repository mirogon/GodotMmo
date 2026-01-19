using Godot;
using System;

public partial class DamageNumber : Node3D
{
    float _lifetime = 0.0f;
    public override void _Ready()
    {
        base._Ready();
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        _lifetime += (float)delta;
        if(_lifetime >= 1.0f)
        {
            QueueFree();
        }
    }
}
