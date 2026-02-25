using Godot;
using System;

public partial class Stone : GameEntity
{
    public Guid Id = Guid.Empty;
    public HealthSystem HealthSystem;

    bool _initialized = false;


    public void Init(Guid id, int currentHealth, int maxHealth)
    {
        Id = id;
        HealthSystem = GetNode("HealthSystem") as HealthSystem;
        HealthSystem.Died += OnDied;
        HealthSystem.MaxHealth = maxHealth;
        HealthSystem.CurrentHealth = currentHealth;

        _initialized = true;
    }
    public override void _Ready()
    {
        base._Ready();
    }

    void OnDied()
    {
        if(this == null || IsQueuedForDeletion()) { return; }
        QueueFree();
        HealthSystem.Died -= OnDied;
    }
}
