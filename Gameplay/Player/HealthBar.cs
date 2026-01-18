using Godot;
using System;

public partial class HealthBar : ProgressBar
{
    HealthSystem _healthSystem;
    public override void _Ready()
    {
        base._Ready();
        _healthSystem = GetParent<HealthSystem>();
        _healthSystem.HealthChanged += OnHealthChange;
    }

    private void OnHealthChange(int health)
    {
        GD.Print("OnHealthChanged: " + health);
        var healthPercentage = (float)((float)health / (float)_healthSystem.MaxHealth) * 100.0f;
        Value = healthPercentage;
    }
}
