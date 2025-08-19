using Godot;
using System;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Concurrent;
using System.Threading;
using Godot.NativeInterop;
using System.Net.Sockets;
using System.Diagnostics;

public partial class MainScene : Node3D
{
    public override void _Ready()
    {
        NetworkClient.StartClient();
    }
}
