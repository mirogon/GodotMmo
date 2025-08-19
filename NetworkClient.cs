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

public class NetworkClient
{
    public static ConcurrentQueue<Packet> PacketsToSend = new();
    static NetManager _client;
    static NetPeer _serverPeer;
    public static UInt64 SessionId;

    static bool _isRunning = true;

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
            byte packetTypeByte = dataReader.GetByte();
            EPacketType packetType = (EPacketType)packetTypeByte;

            switch (packetType)
            {
                case EPacketType.SC_Register: Handle_SC_RegisterPacket(dataReader); break;
            }

            dataReader.Recycle();
        };

        while (_isRunning)
        {
            _client.PollEvents();
            Packet packetRaw;
            if(PacketsToSend.TryDequeue(out packetRaw))
            {
                SendPacket(packetRaw, _serverPeer);
            }
            Thread.Sleep(15);
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
        SessionId = receivedPacket.SessionId;
        GD.Print("RegisterPacket received with SessionId: " + SessionId);
    }

}
