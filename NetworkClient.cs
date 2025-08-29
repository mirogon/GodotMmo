using Godot;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class PeerPlayer
{
    public UInt64 PublicId;
    public Vector3 Position;
    public float YRotationEuler;

    public PeerPlayer(ulong publicId, Vector3 position, float yRotationEuler)
    {
        PublicId = publicId;
        Position = position;
        YRotationEuler = yRotationEuler;
    }
}
public class NetworkClient
{
    public static Action<List<PeerPlayer>> PlayerUpdate;
    public static ConcurrentQueue<Packet> PacketsToSend = new();
    static NetManager _client;
    static NetPeer _serverPeer;
    public static UInt64 SessionId;

    static bool _isRunning = true;

    static Dictionary<UInt64, PeerPlayer> _otherPlayers = new();

    public static void StartClient()
    {
        Thread t = new(new ThreadStart(_Start));
        t.Start();
    }
    
    static void _Start()
    {
        EventBasedNetListener listener = new();
        _client = new NetManager(listener);
        _client.Start();
        _serverPeer = _client.Connect("localhost", 9050, "SomeConnectionKey");
        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
        {
            byte packetTypeByte = dataReader.PeekByte();
            EPacketType packetType = (EPacketType)packetTypeByte;

            switch (packetType)
            {
                case EPacketType.SC_Register: Handle_SC_RegisterPacket(dataReader); break;
                case EPacketType.SC_PlayerUpdate: Handle_SC_PlayerUpdate(dataReader); break;
            }

            dataReader.Recycle();
        };

        Thread.Sleep(100);
        RegisterNetworkClient();

        while (_isRunning)
        {
            _client.PollEvents();
            Packet packetRaw;
            if(PacketsToSend.TryDequeue(out packetRaw))
            {
                SendPacket(packetRaw, _serverPeer);
            }
            Thread.Sleep(5);
        }
        _client.Stop();
    }


    static void SendPacket(Packet packet, NetPeer peer)
    {
        NetDataWriter writer = new();
        var packetRaw = packet.ToByteArray();
        writer.Put(packetRaw);
        peer.Send(writer, DeliveryMethod.ReliableUnordered);
        GD.Print("Sending packet: " + packet.PacketType.ToString() + " Size:" + packetRaw.Length);
    }

    static void Handle_SC_RegisterPacket(NetPacketReader packetReader)
    {
        var byteLen = SC_RegisterPacket.ByteSize;
        byte[] packetData = new byte[byteLen];
        packetReader.GetBytes(packetData, byteLen);
        SC_RegisterPacket receivedPacket = new(packetData);
        SessionId = receivedPacket.Id;
        GD.Print("RegisterPacket received with SessionId: " + SessionId);
    }

    static void Handle_SC_PlayerUpdate(NetPacketReader dataReader)
    {
        var byteLen = SC_PlayerUpdatePacket.ByteSize;
        byte[] packetData = new byte[byteLen];
        dataReader.GetBytes(packetData, byteLen);
        SC_PlayerUpdatePacket receivedPacket = new(packetData);

        PeerPlayer pp = new(receivedPacket.PublicId, new Vector3(receivedPacket.X, receivedPacket.Y, receivedPacket.Z), receivedPacket.YRotationEuler);
        if (!_otherPlayers.ContainsKey(receivedPacket.PublicId))
        {
            _otherPlayers.Add(receivedPacket.PublicId, pp);
        }
        else
        {
            _otherPlayers[receivedPacket.PublicId] = pp;
        }

        GD.Print("Num OtherPlayers: " + _otherPlayers.Count);
        GD.Print("Received PlayerUpdate packet Pos: " + "X:" + receivedPacket.X + " Y:" + receivedPacket.Y + " Z:" + receivedPacket.Z);
        PlayerUpdate?.Invoke(_otherPlayers.Values.ToList());
    }
    static void RegisterNetworkClient() {
        CS_RegisterPacket registerPacket = new();
        NetworkClient.PacketsToSend.Enqueue(registerPacket);
        GD.Print("Sent register packet");
    }
}
