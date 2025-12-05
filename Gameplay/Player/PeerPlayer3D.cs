using Godot;
using System;
using System.Collections.Generic;

public partial class PeerPlayer3D : Node3D
{
    List<ServerMovementSnapshot> _snapshots = new();
    Vector3 _targetPos;
    long _renderTime = 0;
    long _renderTimeDelayMs = 150;

    public override void _Process(double delta)
    {
        base._Process(delta);
        SimpleMovement(delta);
    }
    void SimpleMovement(double delta)
    {
        if(_snapshots.Count < 1) { return; }

        if(GlobalPosition.DistanceTo(_targetPos) < 0.05f)
        {
            return;
        }

        GlobalPosition = GlobalPosition.Lerp(_targetPos, (float)delta * (_snapshots[_snapshots.Count-1].MoveSpeed));
    }

    public void OnMovementUpdate(Vector3 serverPos, Vector3 moveDir, float moveSpeed, bool isMoving, long serverTime)
    {
        ServerMovementSnapshot snapshot = new(serverPos, moveDir, moveSpeed, serverTime, isMoving);
        _snapshots.Add(snapshot);

        if(_snapshots.Count > 10)
        {
            _snapshots.RemoveAt(0);
        }

        long diff = Math.Abs(_renderTime - (snapshot.ServerTimeUtcUnixMs - _renderTimeDelayMs));
        if(_renderTime == 0 || diff > 1000)
        {
            _renderTime = serverTime - _renderTimeDelayMs;
        }

        _targetPos = serverPos;
    }
}
