using Godot;
using System;
using System.Diagnostics;

public partial class Player : Node3D
{
    public float MovementSpeed = 1.0f;

    Stopwatch _posUpdateStopwatch = new();

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
            moveDir += Vector3.Forward;
        }
        if (Input.IsPhysicalKeyPressed(Key.S))
        {
            moveDir += Vector3.Back;
        }
        if (Input.IsPhysicalKeyPressed(Key.A))
        {
            moveDir += Vector3.Left;
        }
        if (Input.IsPhysicalKeyPressed(Key.D))
        {
            moveDir += Vector3.Right;
        }
        moveDir = moveDir.Normalized();

        Position += moveDir * MovementSpeed * (float)delta;
        if(_posUpdateStopwatch.ElapsedMilliseconds >= 1000)
        {
            SendPositionUpdate();
            _posUpdateStopwatch.Restart();
        }
    }

    void SendPositionUpdate()
    {
        CS_PositionUpdate posUpdate = new(NetworkClient.SessionId, Position.X, Position.Y, Position.Z);
        NetworkClient.PacketsToSend.Enqueue(posUpdate);
    }
}
