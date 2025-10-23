using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
    [Export]
    public PackedScene PeerPlayerScene;

    public static PackedScene InfoWindowScreen = ResourceLoader.Load<PackedScene>("res://Scenes/InfoWindow.tscn");
    public static PackedScene GameScene = ResourceLoader.Load<PackedScene>("res://Scenes/MainScene.tscn");
    public static PackedScene SelectCharacterScene = ResourceLoader.Load<PackedScene>("res://Scenes/SelectCharacterScene.tscn");

    List<Node3D> _peerInstances = new();

    public override void _Ready()
    {
        NetworkClient.StartClient();
        NetworkClient.PlayerUpdate += OnPlayerUpdate;

    }
    void OnPlayerUpdate(List<PeerPlayer> peers)
    {
        foreach(var pi in _peerInstances)
        {
            pi.QueueFree();
        }
        _peerInstances.Clear();


        var instance = PeerPlayerScene.Instantiate();
        CallDeferred("add_child", instance);
        //GetParent().AddChild(instance);
        var n3d = instance as Node3D;
        _peerInstances.Add(n3d);
        n3d.Position = peers[0].Position;
        var rot = n3d.Rotation;
        rot.Y = Mathf.DegToRad(peers[0].YRotationEuler);
        n3d.Rotation = rot;
        GD.Print("OnPlayerUpdate PeerPos: X:" + n3d.Position.X +  " Y:" + n3d.Position.Y + " Z:" + n3d.Position.Z);
    }
}
