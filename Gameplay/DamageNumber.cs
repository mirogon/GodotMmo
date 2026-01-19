using Godot;
using System;

public partial class DamageNumber : Node3D
{
    float _lifetime = 0.0f;
    Label _dmgLabel;
    public override void _Ready()
    {
        base._Ready();
        _dmgLabel = FindChild("Label") as Label;
    }

    public void Init(int dmg)
    {
        _dmgLabel = FindChild("Label") as Label;
        _dmgLabel.Text = dmg.ToString();
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
