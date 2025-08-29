using Godot;
using System;
using System.Diagnostics;

public partial class Player : Node3D
{
    public float MovementSpeed = 4.0f;

    [Export] Node3D _playerMesh;

    Stopwatch _posUpdateStopwatch = new();

    const float _positionUpdateIntervalMs = 100.0f;

    public override void _Ready()
    {
        base._Ready();
        _posUpdateStopwatch.Start();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        Vector3 moveDir = Vector3.Zero;
        if (Input.IsPhysicalKeyPressed(Key.W))
        {
            moveDir += -_playerMesh.GlobalTransform.Basis.Z;
        }
        if (Input.IsPhysicalKeyPressed(Key.S))
        {
            moveDir += _playerMesh.GlobalTransform.Basis.Z;
        }
        if (Input.IsPhysicalKeyPressed(Key.A))
        {
            moveDir += -_playerMesh.GlobalTransform.Basis.X;
        }
        if (Input.IsPhysicalKeyPressed(Key.D))
        {
            moveDir += _playerMesh.GlobalTransform.Basis.X;
        }
        moveDir = moveDir.Normalized();

        Position += moveDir * MovementSpeed * (float)delta;
        if(_posUpdateStopwatch.ElapsedMilliseconds >= _positionUpdateIntervalMs)
        {
            SendPositionUpdate();
            _posUpdateStopwatch.Restart();
        }
    }

    void SendPositionUpdate()
    {
        var yDegrees = Mathf.RadToDeg(_playerMesh.Rotation.Y);
        GD.Print("YROT: " + yDegrees);
        CS_PositionUpdate posUpdate = new(NetworkClient.SessionId, Position.X, Position.Y, Position.Z, yDegrees);
        NetworkClient.PacketsToSend.Enqueue(posUpdate);
    }
}
