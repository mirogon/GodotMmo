using Godot;
using System;

public partial class HealthSystem : Node 
{
    public Action<int> HealthChanged;
    public Action Died;
    public int MaxHealth = 100;
    public int CurrentHealth
    {
        get { return _currentHealth; }
        set
        {
            _currentHealth = value;
            if(_currentHealth <= 0)
            {
                _currentHealth = 0;
                _isDead = true;
                HealthChanged?.Invoke(CurrentHealth);
                Died?.Invoke();
                return;
            }
            HealthChanged?.Invoke(CurrentHealth);
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

    int _maxHealth = 100;
    int _currentHealth;
    bool _isDead = false;

    public override void _Ready()
    {
        base._Ready();
    }
}
