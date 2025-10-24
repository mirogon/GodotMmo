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
    public static Action<bool> LoginAttemptUpdate;
    public static Action KnownCharactersUpdate;

    public static ConcurrentQueue<Packet> PacketsToSend = new();
    static NetManager _client;
    static NetPeer _serverPeer;

    public static Dictionary<int, Character> KnownCharacters = new(); //Slot, Char

    static bool _startedClient = false;
    public static bool SuccessfullyLoggedIn = false;
    static bool _isRunning = true;
    static long _packetsSent = 0;

    static Dictionary<UInt64, PeerPlayer> _otherPlayers = new();

    public static void StartClient()
    {
        if (_startedClient) { return; }
        Thread t = new(new ThreadStart(_Start));
        t.Start();
        _startedClient = true;
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
                case EPacketType.SC_CharactersStart: Handle_SC_CharactersStartPacket(dataReader); break;
                case EPacketType.SC_Character: Handle_SC_CharacterPacket(dataReader); break;
                case EPacketType.SC_CharactersEnd: Handle_SC_CharactersEndPacket(dataReader); break;
            }

            dataReader.Recycle();
        };

        Thread.Sleep(100);
        RegisterNetworkClient();

        while (_isRunning)
        {
            _client.PollEvents();

            if (!SuccessfullyLoggedIn && _packetsSent > 0) { continue; }

            Packet packetRaw;
            if(PacketsToSend.TryDequeue(out packetRaw))
            {
                SendPacket(packetRaw, _serverPeer);
                ++_packetsSent;
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

    static void RegisterNetworkClient() {
        CS_RegisterPacket registerPacket = new(LoginClient.NewestSessionId);
        NetworkClient.PacketsToSend.Enqueue(registerPacket);
        GD.Print("Sent register packet");
    }

    public static void CreateNewCharacter(byte slot, string charName, ECharacterClass charClass)
    {
        CS_CreateCharacterPacket createCharPacket = new(LoginClient.NewestSessionId, slot, charName, charClass);
        NetworkClient.PacketsToSend.Enqueue(createCharPacket);
    }

    static void Handle_SC_RegisterPacket(NetPacketReader packetReader)
    {
        var byteLen = SC_RegisterPacket.ByteSize;
        byte[] packetData = new byte[byteLen];
        packetReader.GetBytes(packetData, byteLen);
        SC_RegisterPacket receivedPacket = new(packetData);
        SuccessfullyLoggedIn = receivedPacket.Success;

        LoginAttemptUpdate?.Invoke(SuccessfullyLoggedIn);

        if (SuccessfullyLoggedIn)
        {
            CS_RequestCharactersPacket reqCharsPacket = new(LoginClient.NewestSessionId);
            NetworkClient.PacketsToSend.Enqueue(reqCharsPacket);
        }
    }

    static void Handle_SC_CharactersStartPacket(NetPacketReader packetReader)
    {
        var byteLen = SC_CharactersStartPacket.ByteSize;
        byte[] packetData = new byte[byteLen];
        packetReader.GetBytes(packetData, byteLen);
        KnownCharacters.Clear();
    }

    static void Handle_SC_CharacterPacket(NetPacketReader packetReader)
    {
        var byteLen = SC_CharacterPacket.ByteSize;
        byte[] packetData = new byte[byteLen];
        packetReader.GetBytes(packetData, byteLen);

        SC_CharacterPacket charPacket = new(packetData);

        GD.Print("CHAR FROM SERVER WITH SLOT: " + charPacket.Slot);

        KnownCharacters.Add(charPacket.Slot, new Character(charPacket.Slot, charPacket.Name, charPacket.Class, charPacket.Level, charPacket.Exp));
        GD.Print("New Characater received: " +  charPacket.Name);
    }


    static void Handle_SC_CharactersEndPacket(NetPacketReader packetReader)
    {
        var byteLen = SC_CharactersEndPacket.ByteSize;
        byte[] packetData = new byte[byteLen];
        packetReader.GetBytes(packetData, byteLen);
        KnownCharactersUpdate?.Invoke();
        GD.Print("Known Character Update Invoked");
    }

    static void Handle_SC_PlayerUpdate(NetPacketReader dataReader)
    {
        var byteLen = SC_PlayerUpdatePacket.ByteSize;
        byte[] packetData = new byte[byteLen];
        dataReader.GetBytes(packetData, byteLen);
        SC_PlayerUpdatePacket receivedPacket = new(packetData);

        PeerPlayer pp = new(receivedPacket.SessionId, new Vector3(receivedPacket.X, receivedPacket.Y, receivedPacket.Z), receivedPacket.YRotationEuler);
        if (!_otherPlayers.ContainsKey(receivedPacket.SessionId))
        {
            _otherPlayers.Add(receivedPacket.SessionId, pp);
        }
        else
        {
            _otherPlayers[receivedPacket.SessionId] = pp;
        }

        GD.Print("Num OtherPlayers: " + _otherPlayers.Count);
        GD.Print("Received PlayerUpdate packet Pos: " + "X:" + receivedPacket.X + " Y:" + receivedPacket.Y + " Z:" + receivedPacket.Z);
        PlayerUpdate?.Invoke(_otherPlayers.Values.ToList());
    }
}
