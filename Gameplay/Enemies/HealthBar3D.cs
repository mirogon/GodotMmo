using Godot;
using System;

public partial class HealthBar3D : ProgressBar
{
    HealthSystem _healthSystem;
    public override void _Ready()
    {
        base._Ready();
        _healthSystem = GetParent().GetParent().GetNode<HealthSystem>("HealthSystem");
        _healthSystem.HealthChanged += OnHealthChanged;
    }

    void OnHealthChanged(int newHealth)
    {

        GD.Print("HealthBad3DHEalthChanged: " + newHealth);
        int healthPercentage = (int)( ((float)newHealth / (float)_healthSystem.MaxHealth) * 100 );
        GD.Print("HealthBad3DHealthPercent: " + healthPercentage);
        Value = healthPercentage;
    }
}
