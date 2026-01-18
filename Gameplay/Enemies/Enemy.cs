using Godot;
using System;
using System.Collections.Generic;


public class ServerMovementSnapshot
{
    public Vector3 Position;
    public Vector3 MoveDir;
    public float MoveSpeed;
    public float YRotationEuler;
    public long ServerTimeUtcUnixMs;
    public bool IsMoving;

    public ServerMovementSnapshot(Vector3 position, Vector3 moveDir, float moveSpeed, float yRotationEuler, long serverTime, bool isMoving)
    {
        Position = position;
        MoveDir = moveDir;
        MoveSpeed = moveSpeed;
        YRotationEuler = yRotationEuler;
        ServerTimeUtcUnixMs = serverTime;
        IsMoving = isMoving;
    }
}

public partial class Enemy : Node3D
{
    public HealthSystem HealthSystem;

    EnemyFbx _enemyModel;

    List<ServerMovementSnapshot> _snapshots = new();
    int renderTimeDelayMs = 150;
    long renderTime = 0;

    int usedRealSnapshots = 0;
    int usedPredSnapshots = 0;

    public void Init(int currentHealth, int maxHealth)
    {
        HealthSystem = GetNode("HealthSystem") as HealthSystem;
        HealthSystem.CurrentHealth = currentHealth;
        HealthSystem.MaxHealth = maxHealth;
    }
    public override void _Ready()
    {
        base._Ready();
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
        SimpleMovement(delta);
        return;
    }

    public void PlayAnimation(MonsterAnimationType type)
    {
        _enemyModel.PlayAnimation(type);
    }

    void NotSoSimpleMovement(double delta)
    {
        if(_snapshots.Count < 2) { return; }

        renderTime += (long)(delta * 1000);

        if (!_snapshots[_snapshots.Count-1].IsMoving)
        {
            GlobalPosition = _snapshots[_snapshots.Count-1].Position;
            GD.Print("REAL: " + usedRealSnapshots + " PRED: " + usedPredSnapshots);
            return;
        }

        var afterRenderSnapshot = FindSnapshotRightAfterRenderTimer();
        var beforeRenderSnapshot = FindSnapshotRightBeforeRenderTime();
        if(afterRenderSnapshot == beforeRenderSnapshot) {
            var timeDiffMs = renderTime - afterRenderSnapshot.ServerTimeUtcUnixMs;
            var timeDiffSec = (float)(timeDiffMs) / 1000f;
            var pos = afterRenderSnapshot.Position + afterRenderSnapshot.MoveDir * afterRenderSnapshot.MoveSpeed * timeDiffSec;
            afterRenderSnapshot = new ServerMovementSnapshot(pos, afterRenderSnapshot.MoveDir, afterRenderSnapshot.MoveSpeed, afterRenderSnapshot.YRotationEuler, afterRenderSnapshot.ServerTimeUtcUnixMs + timeDiffMs , afterRenderSnapshot.IsMoving);
            usedPredSnapshots++;
        }
        else
        {
            usedRealSnapshots++;
        }

        long diff = Math.Abs(renderTime - (afterRenderSnapshot.ServerTimeUtcUnixMs - renderTimeDelayMs));

        float alpha = (float)(renderTime - beforeRenderSnapshot.ServerTimeUtcUnixMs) / (float)(afterRenderSnapshot.ServerTimeUtcUnixMs - beforeRenderSnapshot.ServerTimeUtcUnixMs);
        float alphaClamp = Mathf.Clamp(alpha, 0f, 1f);
        var renderPos = beforeRenderSnapshot.Position.Lerp(afterRenderSnapshot.Position, alphaClamp);

        if(alpha > 1.0)
        {
            renderPos = afterRenderSnapshot.Position + afterRenderSnapshot.MoveDir * afterRenderSnapshot.MoveSpeed * (float)((renderTime - afterRenderSnapshot.ServerTimeUtcUnixMs)/1000f);
        }
        //GD.Print("alpa: ", alpha);

        GlobalPosition = renderPos;
    }

    ServerMovementSnapshot FindSnapshotRightBeforeRenderTime()
    {
        ServerMovementSnapshot ret = _snapshots[0];
        foreach(var current in _snapshots)
        {
            if(current.ServerTimeUtcUnixMs < renderTime && current.ServerTimeUtcUnixMs > ret.ServerTimeUtcUnixMs)
            {
                ret = current;
            }
        }
        return ret;
    }
    ServerMovementSnapshot FindSnapshotRightAfterRenderTimer()
    {
        ServerMovementSnapshot ret = _snapshots[_snapshots.Count-1];
        foreach(var current in _snapshots)
        {
            if(current.ServerTimeUtcUnixMs > renderTime && current.ServerTimeUtcUnixMs < ret.ServerTimeUtcUnixMs)
            {
                ret = current;
            }
        }
        return ret;
    }


    public void MovementUpdate(Vector3 serverPos, Vector3 moveDir, float moveSpeed, float yRot, bool isMoving, long serverTime)
    {
        ServerMovementSnapshot snapshot = new(serverPos, moveDir, moveSpeed, yRot, serverTime, isMoving);
        _snapshots.Add(snapshot);

        if(_snapshots.Count > 10)
        {
            _snapshots.RemoveAt(0);
        }

        long diff = Math.Abs(renderTime - (snapshot.ServerTimeUtcUnixMs - renderTimeDelayMs));
        if(renderTime == 0 || diff > 1000)
        {
            renderTime = serverTime - renderTimeDelayMs;
        }

        _targetPos = serverPos;
    }

    Vector3 _targetPos;
    void SimpleMovement(double delta)
    {
        if(_snapshots.Count < 1) { return; }

        if(GlobalPosition.DistanceTo(_targetPos) < 0.05f)
        {
            _enemyModel?.PlayAnimation(MonsterAnimationType.Idle);
            return;
        }

        GlobalPosition = GlobalPosition.Lerp(_targetPos, (float)delta * (_snapshots[0].MoveSpeed * 0.95f));

        _enemyModel?.PlayAnimation(MonsterAnimationType.Walk);
    }
}
