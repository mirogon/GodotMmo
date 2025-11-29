using Godot;
using System;

public partial class Enemy : Node3D
{
    HealthSystem _healthSystem;

    public override void _Ready()
    {
        base._Ready();
        _healthSystem = GetNode("HealthSystem") as HealthSystem;
    }
}
