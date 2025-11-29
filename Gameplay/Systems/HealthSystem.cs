using Godot;
using System;

public partial class HealthSystem : Node 
{
    public Action<int> HealthChange;
    public int MaxHealth {
        get { return _maxHealth; }
        set {  _maxHealth = value; } 
    }
    public int CurrentHealth
    {
        get { return _currentHealth; }
        set { 
            _currentHealth = value;
            HealthChange?.Invoke(CurrentHealth);
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

    public override void _Ready()
    {
        base._Ready();
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = currentHealth;
    }
}
