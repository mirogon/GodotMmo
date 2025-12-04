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
using System.Xml;


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
    public static Action<List<MongoMapItem>> NewItemsOnMapUpdate;
    public static Action<List<Guid>> RemovedItemsOnMap;
    public static Action KnownItemsUpdate;
    public static Action<(UInt64 publicId, int currentHealth, int maxHealth)> CharacterHealthUpdate;

    public static Action EnemiesOnMapUpdate;
    public static ConcurrentBag<EnemyData> NewestEnemiesOnMapUpdate = new();

    public static Action MonsterPositionUpdate;
    public static SC_MonsterPositionUpdatePacket NewerstMonsterPosUpdate = new();

    public static Action MonstersHealthUpdate;
    public static ConcurrentQueue<SC_MonstersHealthUpdate> MonstersHealthUpdateQueue = new();

    public static ConcurrentQueue<(Packet packet, Type type)> ReliableUnorderedPacketsToSend = new();
    static NetManager _client;
    static NetPeer _serverPeer;

    public static UInt64 PublicId;

    public static Dictionary<int, Character> KnownCharacters = new(); //Slot, Char
    public static List<MongoInventoryItem> KnownInventoryItems = new();

    static bool _startedClient = false;
    public static bool SuccessfullyLoggedIn = false;
    static bool _isRunning = true;
    static long _packetsSent = 0;


    //Public ID, PeerPlayer
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
            short packetTypeByte = dataReader.PeekShort();
            EPacketType packetType = (EPacketType)packetTypeByte;

            switch (packetType)
            {
                case EPacketType.SC_Register: Handle_SC_RegisterPacket(dataReader); break;
                case EPacketType.SC_CharacterUpdate: Handle_SC_CharacterUpdate(dataReader); break;
                case EPacketType.SC_CharactersStart: Handle_SC_CharactersStartPacket(dataReader); break;
                case EPacketType.SC_Character: Handle_SC_CharacterPacket(dataReader); break;
                case EPacketType.SC_CharactersEnd: Handle_SC_CharactersEndPacket(dataReader); break;
                case EPacketType.SC_MapItemsAddedUpdate: Handle_SC_MapItemsUpdate(dataReader); break;
                case EPacketType.SC_MapItemsRemovedUpdate: Handle_SC_MapItemsRemovedUpdate(dataReader); break;
                case EPacketType.SC_CharacterInventoryItemsUpdateStart: Handle_SC_InventoryItemsUpdateStart(dataReader); break;
                case EPacketType.SC_CharacterInventoryItemsUpdate: Handle_SC_InventoryItemsUpdate(dataReader); break;
                case EPacketType.SC_CharacterInventoryItemsUpdateEnd: Handle_SC_InventoryItemsUpdateEnd(dataReader); break;
                case EPacketType.SC_CharacterHealthUpdate: Handle_SC_CharacterHealthUpdate(dataReader); break;
                case EPacketType.SC_EnemiesOnMap: Handle_SC_EnemiesOnMapPacket(dataReader); break;
                case EPacketType.SC_MonsterPositionUpdate: Handle_SC_MonsterPositionUpdate(dataReader); break;
                case EPacketType.SC_MonstersHealthUpdate: HandleSC_MonstersHealthUpdate(dataReader); break;
            }

            dataReader.Recycle();
        };

        Thread.Sleep(100);
        RegisterNetworkClient();

        while (_isRunning)
        {
            _client.PollEvents();

            if (!SuccessfullyLoggedIn && _packetsSent > 0) { continue; }

            (Packet packet, Type type) packetRaw;
            if(ReliableUnorderedPacketsToSend.TryDequeue(out packetRaw))
            {
                NetworkPacketUtil.SendPacketReliableUnordered(packetRaw.packet, packetRaw.type, _serverPeer);
                ++_packetsSent;
            }
            Thread.Sleep(5);
        }
        _client.Stop();
    }


    static void RegisterNetworkClient() {
        CS_RegisterPacket registerPacket = new(LoginClient.NewestSessionId);
        NetworkClient.ReliableUnorderedPacketsToSend.Enqueue((registerPacket, typeof(CS_RegisterPacket)));
        GD.Print("Sent register packet");
    }

    public static void CreateNewCharacter(byte slot, string charName, ECharacterClass charClass)
    {
        CS_CreateCharacterPacket createCharPacket = new(LoginClient.NewestSessionId, slot, charName, charClass);
        NetworkClient.ReliableUnorderedPacketsToSend.Enqueue((createCharPacket, typeof(CS_CreateCharacterPacket)));
    }

    public static void DeleteCharacter(byte slot)
    {
        CS_DeleteCharacterPacket delPacket = new(LoginClient.NewestSessionId, slot);
        NetworkClient.ReliableUnorderedPacketsToSend.Enqueue((delPacket,typeof(CS_DeleteCharacterPacket)));
    }

    public static void GetCharactersUpdate()
    {
        CS_RequestCharactersPacket reqCharsPacket = new(LoginClient.NewestSessionId);
        NetworkClient.ReliableUnorderedPacketsToSend.Enqueue((reqCharsPacket, typeof(CS_RequestCharactersPacket)));
    }

    public static void PickUpItem(Guid itemId)
    {
        CS_PickUpItemPacket packet = new(LoginClient.NewestSessionId, itemId);
        NetworkClient.ReliableUnorderedPacketsToSend.Enqueue((packet, typeof(CS_PickUpItemPacket)));
    }

    public static void MoveItem(Guid itemId, int newTilePosX, int newTilePosY)
    {
        CS_ItemMovedPacket p = new(LoginClient.NewestSessionId, itemId, (byte)newTilePosX, (byte)newTilePosY);
        NetworkClient.ReliableUnorderedPacketsToSend.Enqueue((p, typeof(CS_ItemMovedPacket)));
    }

    public static void ThrowAwayItem(Guid itemId)
    {
        CS_ThrowAwayItemPacket throwAwayPacket = new(LoginClient.NewestSessionId, itemId);
        NetworkClient.ReliableUnorderedPacketsToSend.Enqueue((throwAwayPacket, typeof(CS_ThrowAwayItemPacket)));
    }

    public static void WeaponAttackMonsters(List<Guid> monsters)
    {
        CS_CharacterMonsterAttackPacket packet = new(LoginClient.NewestSessionId, CharacterAttackType.WeaponAttack, monsters);
        NetworkClient.ReliableUnorderedPacketsToSend.Enqueue((packet, typeof(CS_CharacterMonsterAttackPacket)));
    }

    static void Handle_SC_RegisterPacket(NetPacketReader packetReader)
    {
        SC_RegisterPacket receivedPacket = NetworkPacketUtil.PacketBytesToPacketObject<SC_RegisterPacket>(packetReader);
        SuccessfullyLoggedIn = receivedPacket.Success;

        PublicId = receivedPacket.PublicId;

        LoginAttemptUpdate?.Invoke(SuccessfullyLoggedIn);

        if (SuccessfullyLoggedIn)
        {
            GetCharactersUpdate();
        }
    }

    static void Handle_SC_CharactersStartPacket(NetPacketReader packetReader)
    {
        var byteLen = new SC_CharactersStartPacket().ByteSize;
        byte[] packetData = new byte[byteLen];
        packetReader.GetBytes(packetData, byteLen);
        KnownCharacters.Clear();
    }

    static void Handle_SC_CharacterPacket(NetPacketReader packetReader)
    {
        SC_CharacterPacket charPacket = NetworkPacketUtil.PacketBytesToPacketObject<SC_CharacterPacket>(packetReader);

        GD.Print("CHAR FROM SERVER WITH SLOT: " + charPacket.Slot);

        KnownCharacters.Add(charPacket.Slot, new Character(
            charPacket.Slot, 
            charPacket.Name.ToString(), 
            charPacket.Class, 
            charPacket.Level, 
            charPacket.Exp, 
            charPacket.CurrentlyOnMap, 
            charPacket.PositionOnMap,
            charPacket.MaxHealth,
            charPacket.CurrentHealth,
            charPacket.IsDead
            )
        );
        GD.Print("New Characater received: " +  charPacket.Name);
    }


    static void Handle_SC_CharactersEndPacket(NetPacketReader packetReader)
    {
        var byteLen = new SC_CharactersEndPacket().ByteSize;
        byte[] packetData = new byte[byteLen];
        packetReader.GetBytes(packetData, byteLen);
        KnownCharactersUpdate?.Invoke();
        GD.Print("Known Character Update Invoked");
    }

    static void Handle_SC_MapItemsUpdate(NetPacketReader dataReader)
    {
        SC_MapItemsUpdatePacket packet = NetworkPacketUtil.PacketBytesToPacketObject<SC_MapItemsUpdatePacket>(dataReader);
       
        foreach(var item in packet.Items)
        {
            if(item.ItemType == ItemType.Unknown) { continue; }
            GD.Print("Item on map: " + item.ItemType.ToString() + " " + item.Id.ToString());
        }
        NewItemsOnMapUpdate?.Invoke(packet.Items.ToList());
    }

    static void Handle_SC_MapItemsRemovedUpdate(NetPacketReader dataReader)
    {
        SC_MapItemsRemovedUpdatePacket packet = NetworkPacketUtil.PacketBytesToPacketObject<SC_MapItemsRemovedUpdatePacket>(dataReader);

        List<Guid> itemsRemoved = new();
        for(int i= 0; i < packet.RemovedItems.Length; ++i)
        {
            var current = packet.RemovedItems[i];
            if(current == Guid.Empty) { continue; }
            itemsRemoved.Add(current);
        }
        RemovedItemsOnMap?.Invoke(itemsRemoved);
    }

    static void Handle_SC_CharacterUpdate(NetPacketReader dataReader)
    {
        SC_CharacterUpdatePacket receivedPacket = NetworkPacketUtil.PacketBytesToPacketObject<SC_CharacterUpdatePacket>(dataReader);

        PeerPlayer pp = new(receivedPacket.PublicId, new Vector3(receivedPacket.X, receivedPacket.Y, receivedPacket.Z), receivedPacket.YRotationEuler);
        if (!_otherPlayers.ContainsKey(receivedPacket.PublicId))
        {
            _otherPlayers.Add(receivedPacket.PublicId, pp);
        }
        else
        {
            _otherPlayers[receivedPacket.PublicId] = pp;
        }

        //GD.Print("Num OtherPlayers: " + _otherPlayers.Count);
        //GD.Print("Received PlayerUpdate packet Pos: " + "X:" + receivedPacket.X + " Y:" + receivedPacket.Y + " Z:" + receivedPacket.Z);
        PlayerUpdate?.Invoke(_otherPlayers.Values.ToList());
    }
    static void Handle_SC_InventoryItemsUpdateStart(NetPacketReader dataReader)
    {
        GD.Print("InventoryUpdateStart");
        dataReader.GetByte();
        KnownInventoryItems.Clear();
    }

    static void Handle_SC_InventoryItemsUpdate(NetPacketReader dataReader)
    {
        GD.Print("InventoryUpdate");
        SC_CharacterInventoryItemsUpdatePacket receivedPacket = NetworkPacketUtil.PacketBytesToPacketObject<SC_CharacterInventoryItemsUpdatePacket>(dataReader);

        GD.Print("Received ItemUpdatePacket NumItems: " + receivedPacket.Items.Count());
        foreach(var item in receivedPacket.Items)
        {
            if(item.ItemType == ItemType.Unknown) { continue; }
            KnownInventoryItems.Add(item);
            GD.Print("Received item: " + item.Name + " " + item.Id);
        }
    }

    static void Handle_SC_InventoryItemsUpdateEnd(NetPacketReader dataReader)
    {
        GD.Print("InventoryUpdateEnd");
        dataReader.GetByte();
        KnownItemsUpdate?.Invoke();
    }

    static void Handle_SC_CharacterHealthUpdate(NetPacketReader dataReader)
    {
        SC_CharacterHealthUpdatePacket receivedPacket = NetworkPacketUtil.PacketBytesToPacketObject<SC_CharacterHealthUpdatePacket>(dataReader);

        if(receivedPacket.PublicId != PublicId) { return; }

        CharacterHealthUpdate?.Invoke((receivedPacket.PublicId, receivedPacket.CurrentHealth, receivedPacket.MaxHealth));
    }

    static void Handle_SC_EnemiesOnMapPacket(NetPacketReader packetReader)
    {
        SC_EnemiesOnMapPacket enemiesUpdatePacket = NetworkPacketUtil.PacketBytesToPacketObject<SC_EnemiesOnMapPacket>(packetReader);
        NewestEnemiesOnMapUpdate = new(enemiesUpdatePacket.Enemies);
        EnemiesOnMapUpdate?.Invoke();
    }

    static void Handle_SC_MonsterPositionUpdate(NetPacketReader packetReader)
    {
        SC_MonsterPositionUpdatePacket packet = NetworkPacketUtil.PacketBytesToPacketObject<SC_MonsterPositionUpdatePacket>(packetReader);
        NewerstMonsterPosUpdate = packet;
        MonsterPositionUpdate?.Invoke();
    }
    static void HandleSC_MonstersHealthUpdate(NetPacketReader dataReader)
    {
        SC_MonstersHealthUpdate packet = NetworkPacketUtil.PacketBytesToPacketObject<SC_MonstersHealthUpdate>(dataReader);
        MonstersHealthUpdateQueue.Enqueue(packet);
        MonstersHealthUpdate?.Invoke();
    }


}
