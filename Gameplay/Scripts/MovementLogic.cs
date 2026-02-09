using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class ServerMovementSnapshot
{
    public Vector3 Position;
    public Vector3 MoveDir;
    public float MoveSpeed;
    public Vector3 LookDir;
    public long ServerTimeUtcUnixMs;
    public bool IsMoving;

    public ServerMovementSnapshot(Vector3 position, Vector3 moveDir, float moveSpeed, Vector3 lookDir, long serverTime, bool isMoving)
    {
        Position = position;
        MoveDir = moveDir;
        MoveSpeed = moveSpeed;
        LookDir = lookDir;
        ServerTimeUtcUnixMs = serverTime;
        IsMoving = isMoving;
    }
}


public partial class MovementLogic
{
    List<ServerMovementSnapshot> _snapshots = new();
    int renderTimeDelayMs = 150;
    long renderTime = 0;

    int usedRealSnapshots = 0;
    int usedPredSnapshots = 0;

    Node3D _node3d;

    Vector3 _targetPos;
    Vector3 _lookDir;

    public MovementLogic(Node3D node3d)
    {
        _node3d = node3d;
    }

    public void MovementUpdate(Vector3 serverPos, Vector3 moveDir, float moveSpeed, Vector3 lookDir, bool isMoving, long serverTime)
    {
        ServerMovementSnapshot snapshot = new(serverPos, moveDir, moveSpeed, lookDir, serverTime, isMoving);
        _snapshots.Add(snapshot);

        if (_snapshots.Count > 10)
        {
            _snapshots.RemoveAt(0);
        }

        long diff = Math.Abs(renderTime - (snapshot.ServerTimeUtcUnixMs - renderTimeDelayMs));
        if (renderTime == 0 || diff > 1000)
        {
            renderTime = serverTime - renderTimeDelayMs;
        }

        _targetPos = serverPos;
        _lookDir = lookDir;
    }

    public bool NotSoSimpleMovement(double delta)
    {
        if (_snapshots.Count < 2) { return false; }

        //STUPID ROT IMPL
        _node3d.LookAt(_node3d.Position + _lookDir);

        renderTime += (long)(delta * 1000);

        if (!_snapshots[_snapshots.Count - 1].IsMoving)
        {
            _node3d.GlobalPosition = _snapshots[_snapshots.Count - 1].Position;
            //GD.Print("REAL: " + usedRealSnapshots + " PRED: " + usedPredSnapshots);
            return false;
        }

        var afterRenderSnapshot = FindSnapshotRightAfterRenderTimer();
        var beforeRenderSnapshot = FindSnapshotRightBeforeRenderTime();
        if (afterRenderSnapshot == beforeRenderSnapshot)
        {
            var timeDiffMs = renderTime - afterRenderSnapshot.ServerTimeUtcUnixMs;
            var timeDiffSec = (float)(timeDiffMs) / 1000f;
            var pos = afterRenderSnapshot.Position + afterRenderSnapshot.MoveDir * afterRenderSnapshot.MoveSpeed * timeDiffSec;
            afterRenderSnapshot = new ServerMovementSnapshot(pos, afterRenderSnapshot.MoveDir, afterRenderSnapshot.MoveSpeed, afterRenderSnapshot.LookDir, afterRenderSnapshot.ServerTimeUtcUnixMs + timeDiffMs, afterRenderSnapshot.IsMoving);
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

        if (alpha > 1.0)
        {
            renderPos = afterRenderSnapshot.Position + afterRenderSnapshot.MoveDir * afterRenderSnapshot.MoveSpeed * (float)((renderTime - afterRenderSnapshot.ServerTimeUtcUnixMs) / 1000f);
        }
        //GD.Print("alpa: ", alpha);

        _node3d.GlobalPosition = renderPos;
        return true;
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

    /// <summary>
    /// <returns>whether movement was made or not</returns>
    public bool SimpleMovement(double delta)
    {
        if(_snapshots.Count < 1) { return false; }

        if(_lookDir.Length() > 0.1f)
        {
            _node3d.LookAt(_node3d.Position + _lookDir);
        }

        if(_node3d.GlobalPosition.DistanceTo(_targetPos) < 0.05f)
        {
            return false;
        }

        var moveSpeed = _snapshots[0].MoveSpeed * 0.99f;
        if(_node3d.GlobalPosition.DistanceTo(_targetPos) > 1.0f)
        {
            moveSpeed = _snapshots[0].MoveSpeed * 1.1f;
        }

        _node3d.GlobalPosition = _node3d.GlobalPosition.Lerp(_targetPos, (float)delta * moveSpeed);
        return true;
    }

}
