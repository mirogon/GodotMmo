using Godot;
using System;
using System.Collections.Generic;


public partial class Enemy : GameEntity
{
    public Guid Id = Guid.Empty;
    public HealthSystem HealthSystem;

    EnemyFbx _enemyModel;

    List<ServerMovementSnapshot> _snapshots = new();
    int renderTimeDelayMs = 150;
    long renderTime = 0;

    int usedRealSnapshots = 0;
    int usedPredSnapshots = 0;

    MovementLogic _movement;

    public void Init(Guid id, int currentHealth, int maxHealth)
    {
        Id = id;
        HealthSystem = GetNode("HealthSystem") as HealthSystem;
        HealthSystem.CurrentHealth = currentHealth;
        HealthSystem.MaxHealth = maxHealth;
    }
    public override void _Ready()
    {
        base._Ready();
        _movement = new(this);

        HealthSystem = GetNode("HealthSystem") as HealthSystem;
        HealthSystem.Died += OnDied;

        _enemyModel = GetNode<EnemyFbx>("Model");
    }

    void OnDied()
    {
        if ( this == null || IsQueuedForDeletion()) { return; }
        QueueFree();
        HealthSystem.Died -= OnDied;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        var movementMade = _movement.SimpleMovement(delta);
        if (movementMade)
        {
            _enemyModel?.PlayAnimation(MonsterAnimationType.Walk);
        }
        else
        {
            _enemyModel?.PlayAnimation(MonsterAnimationType.Idle);
        }
    }

    public void PlayAnimation(MonsterAnimationType type)
    {
        _enemyModel.PlayAnimation(type);
    }

    public void MovementUpdate(Vector3 serverPos, Vector3 moveDir, float moveSpeed, Vector3 lookDir, bool isMoving, long serverTime)
    {
        _movement.MovementUpdate(serverPos, moveDir, moveSpeed, lookDir, isMoving, serverTime);
    }
}
