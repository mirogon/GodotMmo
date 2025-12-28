using Godot;
using System;
using System.Collections.Generic;

public partial class PeerPlayer3D : Node3D
{
    List<ServerMovementSnapshot> _snapshots = new();
    Vector3 _targetPos;
    float _targetYRotEuler;
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

        if (GlobalPosition.DistanceTo(_targetPos) > 0.05f)
        {
            GlobalPosition = GlobalPosition.Lerp(_targetPos, (float)delta * (_snapshots[_snapshots.Count - 1].MoveSpeed));
        }

        if(Mathf.Abs(_targetYRotEuler - RotationDegrees.Y) > 5)
        {
            var targetRot = Quaternion.FromEuler(new Vector3(0, _targetYRotEuler, 0));
            targetRot = targetRot.Normalized();
            Quaternion = Quaternion.Slerp(targetRot, (float)delta * 90.0f);
        }
    }

    public void OnMovementUpdate(Vector3 serverPos, Vector3 moveDir, float moveSpeed, float yRotationEuler, bool isMoving, long serverTime)
    {
        ServerMovementSnapshot snapshot = new(serverPos, moveDir, moveSpeed, yRotationEuler, serverTime, isMoving);
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
        _targetYRotEuler = yRotationEuler;
    }
}
