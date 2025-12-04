using Godot;
using System;
using System.Collections.Generic;


public class EnemyMovementSnapshot
{
    public Vector3 Position;
    public Vector3 Velocity;
    public long ServerTimeUtcUnixMs;
    public bool IsMoving;

    public EnemyMovementSnapshot(Vector3 position, Vector3 velocity, long serverTime, bool isMoving)
    {
        Position = position;
        Velocity = velocity;
        ServerTimeUtcUnixMs = serverTime;
        IsMoving = isMoving;
    }
}

public partial class Enemy : Node3D
{
    public HealthSystem HealthSystem;

    List<EnemyMovementSnapshot> _snapshots = new();
    int renderTimeDelayMs = 150;
    long renderTime = 0;

    int usedRealSnapshots = 0;
    int usedPredSnapshots = 0;
    public override void _Ready()
    {
        base._Ready();
        HealthSystem = GetNode("HealthSystem") as HealthSystem;
        HealthSystem.Died += OnDied;
    }

    void OnDied()
    {
        QueueFree();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        SimpleMovement(delta);
        return;
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
            var pos = afterRenderSnapshot.Position + afterRenderSnapshot.Velocity * timeDiffSec;
            afterRenderSnapshot = new EnemyMovementSnapshot(pos, afterRenderSnapshot.Velocity, afterRenderSnapshot.ServerTimeUtcUnixMs + timeDiffMs , afterRenderSnapshot.IsMoving);
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
            renderPos = afterRenderSnapshot.Position + afterRenderSnapshot.Velocity * (float)((renderTime - afterRenderSnapshot.ServerTimeUtcUnixMs)/1000f);
        }
        //GD.Print("alpa: ", alpha);

        GlobalPosition = renderPos;
    }

    EnemyMovementSnapshot FindSnapshotRightBeforeRenderTime()
    {
        EnemyMovementSnapshot ret = _snapshots[0];
        foreach(var current in _snapshots)
        {
            if(current.ServerTimeUtcUnixMs < renderTime && current.ServerTimeUtcUnixMs > ret.ServerTimeUtcUnixMs)
            {
                ret = current;
            }
        }
        return ret;
    }
    EnemyMovementSnapshot FindSnapshotRightAfterRenderTimer()
    {
        EnemyMovementSnapshot ret = _snapshots[_snapshots.Count-1];
        foreach(var current in _snapshots)
        {
            if(current.ServerTimeUtcUnixMs > renderTime && current.ServerTimeUtcUnixMs < ret.ServerTimeUtcUnixMs)
            {
                ret = current;
            }
        }
        return ret;
    }


    public void MovementUpdate(Vector3 serverPos, Vector3 velocity, bool isMoving, long serverTime)
    {
        EnemyMovementSnapshot snapshot = new(serverPos, velocity, serverTime, isMoving);
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
            return;
        }

        GlobalPosition = GlobalPosition.Lerp(_targetPos, (float)delta * (_snapshots[0].Velocity.Length() * 0.95f));
    }
}
