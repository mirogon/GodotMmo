using Godot;
using System;

public partial class Player : Node3D
{
    public float MovementSpeed = 1.0f;
    public override void _Ready()
    {
        base._Ready();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if(Input.IsActionJustPressed("Enter")){
            RegisterNetworkClient();
        }

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
    }

    void RegisterNetworkClient() {
        CS_RegisterPacket registerPacket = new();
        NetworkClient.PacketsToSend.Enqueue(registerPacket);
        GD.Print("Sent register packet");
    }

    void SendPositionUpdate()
    {
        CS_PositionUpdate posUpdate = new(NetworkClient.SessionId, Position.X, Position.Y, Position.Z);
        NetworkClient.PacketsToSend.Enqueue(posUpdate);
    }
}
