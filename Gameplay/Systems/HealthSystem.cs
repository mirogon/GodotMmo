using Godot;
using System;

public partial class HealthSystem : Control
{
    public int MaxHealth {
        get { return _maxHealth; }
        set {  _maxHealth = value; } 
    }
    public int CurrentHealth
    {
        get { return _currentHealth; }
        set { 
            _currentHealth = value;
            var healthPercentage = (float)((float)_currentHealth / (float)_maxHealth) * 100.0f;
            _healthProgressBar.Value = healthPercentage;
        }
    }

    public bool IsDead
    {
        get
        {
            return _isDead;
        }
        set
        {
            _isDead = value;
        }
    }

    int _maxHealth;
    int _currentHealth;
    bool _isDead = false;

    ProgressBar _healthProgressBar;

    public override void _Ready()
    {
        base._Ready();
        _healthProgressBar = GetNode<ProgressBar>("HealthBar");
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = currentHealth;
    }
}
