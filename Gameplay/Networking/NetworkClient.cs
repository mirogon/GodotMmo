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
    public Vector3 Velocity;
    public bool IsMoving;

    public PeerPlayer(ulong publicId, Vector3 position, float yRotationEuler)
    {
        PublicId = publicId;
        Position = position;
        YRotationEuler = yRotationEuler;
    }
}
public class NetworkClient
{
    public static Action PeerPlayerPositionUpdate;
    public static ConcurrentQueue<SC_PeerPlayerPositionUpdatePacket> PeerPlayerPositionUpdateQueue = new();

    public static Action<bool> LoginAttemptUpdate;
    public static Action<List<MongoMapItem>> NewItemsOnMapUpdate;
    public static Action<List<Guid>> RemovedItemsOnMap;
    public static Action KnownItemsUpdate;
    public static Action<(UInt64 publicId, int currentHealth, int maxHealth)> CharacterHealthUpdate;

    public static Action EnemiesOnMapUpdate;
    public static ConcurrentBag<EnemyData> NewestEnemiesOnMapUpdate = new();

    public static Action MonsterPositionUpdate;
    public static ConcurrentBag<SC_MonsterPositionUpdatePacket> MonsterPosUpdates = new();

    public static Action MonstersHealthUpdate;
    public static ConcurrentBag<SC_MonstersHealthUpdate> MonstersHealthUpdateQueue = new();

    public static Action<UInt64> EquippedItemsUpdate;

    public static Action CharacterExpUpdate;
    public static (int lvl, long exp, short availablePoints) KnownCharacterExp;

    public static Action<ulong, short> CharacterAnimationUpdate; //PublicId, CharacterAnimationType

    public static Action<UInt64> CharacterLoggedOut;

    public static Action MonsterAnimationUpdate;
    public static List<SC_MonsterAnimationUpdatePacket> MonsterAnimationUpdates = new();

    public static Dictionary<int, Character> KnownCharacters = new(); //Slot, Char
    public static Action KnownCharactersUpdate;

    public static Character NewestKnownCharacter;
    public static Action KnownCharacterUpdate;

    public static List<MongoInventoryItem> KnownInventoryItems = new();

    public static Action DamageHitsUpdate;
    public static List<SC_DamageHitsUpdatePacket> DamageHitsUpdates = new();

    public static Action QuestsProgressUpdate;
    public static ConcurrentBag<SC_QuestsUpdatePacket> QuestProgressUpdates = new();

    public static Action NpcUpdate;
    public static ConcurrentBag<SC_NpcUpdatePacket> NpcUpdates = new();

    public static Action MountUpdate;
    public static SC_Mount NewestMountUpdate = new();

    public static ConcurrentQueue<(Packet packet, Type type)> ReliableUnorderedPacketsToSend = new();
    static NetManager _client;
    static NetPeer _serverPeer;

    public static UInt64 PublicId;

    public static Dictionary<UInt64, Dictionary<EquipmentSlot, MongoInventoryItem>> KnownEquippedItems = new();

    static bool _startedClient = false;
    public static bool SuccessfullyLoggedIn = false;
    static bool _isRunning = true;
    static long _packetsSent = 0;

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
                case EPacketType.SC_PeerPlayerPositionUpdate: Handle_SC_PeerPlayerPositionUpdate(dataReader); break;
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
                case EPacketType.SC_EquippedItemsUpdate: Handle_SC_EquippedItemsUpdate(dataReader); break;
                case EPacketType.SC_CharacterAnimationUpdate: Handle_SC_CharacterAnimationUpdate(dataReader); break;
                case EPacketType.SC_CharacterExpUpdate: Handle_SC_CharacterExpUpdatePacket(dataReader); break;
                case EPacketType.SC_CharacterLoggedOut: Handle_SC_CharacterLoggedOut(dataReader); break;
                case EPacketType.SC_MonsterAnimationUpdate: Handle_SC_MonsterAnimationUpdate(dataReader); break;
                case EPacketType.SC_DamageHitsUpdate: Handle_SC_DamageHitsUpdate(dataReader); break;
                case EPacketType.SC_QuestsUpdate: Handle_SC_QuestsUpdate(dataReader); break;
                case EPacketType.SC_NpcUpdate: Handle_SC_NpcUpdate(dataReader); break;
                case EPacketType.SC_Mount: Handle_SC_Mount(dataReader); break;
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

    public static void WeaponAttack()
    {
        CS_CharacterAttackPacket packet = new(LoginClient.NewestSessionId, CharacterAttackType.WeaponAttack);
        NetworkClient.ReliableUnorderedPacketsToSend.Enqueue((packet, typeof(CS_CharacterAttackPacket)));
    }

    public static void EquipOrUnequipItem(Guid itemId, bool equip)
    {
        CS_EquipmentChangePacket packet = new(LoginClient.NewestSessionId, itemId, equip);
        NetworkClient.ReliableUnorderedPacketsToSend.Enqueue((packet, typeof(CS_EquipmentChangePacket)));
    }

    public static void SendCharacterAnimationStart(CharacterAnimationType type)
    {
        CS_CharacterAnimationUpdatePacket packet = new(LoginClient.NewestSessionId, type, AnimationState.Start);
        NetworkClient.ReliableUnorderedPacketsToSend.Enqueue((packet, typeof(CS_CharacterAnimationUpdatePacket)));
    }

    public static void Logout()
    {
        CS_LogoutPacket packet = new(LoginClient.NewestSessionId);
        NetworkClient.ReliableUnorderedPacketsToSend.Enqueue((packet, typeof(CS_LogoutPacket)));
    }

    public static void AcceptQuest(QuestId id)
    {
        CS_AcceptQuestPacket packet = new(LoginClient.NewestSessionId, id);
        NetworkClient.ReliableUnorderedPacketsToSend.Enqueue((packet, typeof(CS_AcceptQuestPacket)));
    }
    public static void CompleteQuest(QuestId id)
    {
        CS_CompleteQuestPacket packet = new(LoginClient.NewestSessionId, id);
        NetworkClient.ReliableUnorderedPacketsToSend.Enqueue((packet, typeof(CS_CompleteQuestPacket)));
    }
    public static void MountUp(MountType type)
    {
        CS_Mount packet = new(LoginClient.NewestSessionId, type);
        NetworkClient.ReliableUnorderedPacketsToSend.Enqueue((packet, typeof(CS_Mount)));
    }

    public static void PingServer()
    {
        CS_PingPacket p = new();
        NetworkClient.ReliableUnorderedPacketsToSend.Enqueue((p, typeof(CS_PingPacket)));
    }

    public static void SendIncreaseStat(StatType type, int amount = 1)
    {
        CS_IncreaseStatPacket packet = new(LoginClient.NewestSessionId, type);
        NetworkClient.ReliableUnorderedPacketsToSend.Enqueue((packet, typeof(CS_IncreaseStatPacket)));
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

        GD.Print("CHAR FROM SERVER WITH SLOT: " + charPacket.CharacterData.Slot);

        if (KnownCharacters.ContainsKey(charPacket.CharacterData.Slot))
        {
            KnownCharacters.Remove(charPacket.CharacterData.Slot);
        }
        KnownCharacters.Add(charPacket.CharacterData.Slot, charPacket.CharacterData);

        NewestKnownCharacter = charPacket.CharacterData;
        KnownCharacterUpdate?.Invoke();
        GD.Print("New Characater received: " +  charPacket.CharacterData.Name.ToString());
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

    static void Handle_SC_PeerPlayerPositionUpdate(NetPacketReader dataReader)
    {
        SC_PeerPlayerPositionUpdatePacket receivedPacket = NetworkPacketUtil.PacketBytesToPacketObject<SC_PeerPlayerPositionUpdatePacket>(dataReader);

        PeerPlayerPositionUpdateQueue.Enqueue(receivedPacket);
        PeerPlayerPositionUpdate?.Invoke();
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

        //if(receivedPacket.PublicId != PublicId) { return; }

        CharacterHealthUpdate?.Invoke((receivedPacket.PublicId, receivedPacket.CurrentHealth, receivedPacket.MaxHealth));
    }

    static void Handle_SC_EnemiesOnMapPacket(NetPacketReader packetReader)
    {
        SC_EnemiesOnMapPacket enemiesUpdatePacket = NetworkPacketUtil.PacketBytesToPacketObject<SC_EnemiesOnMapPacket>(packetReader);
        foreach(var enemy in enemiesUpdatePacket.Enemies)
        {
            if(enemy.Id == Guid.Empty) { continue; }
            NewestEnemiesOnMapUpdate.Add(enemy);
        }
        GD.Print("NUM ENEMIES UPDATE: " + NewestEnemiesOnMapUpdate.Count);
        EnemiesOnMapUpdate?.Invoke();
    }

    static void Handle_SC_MonsterPositionUpdate(NetPacketReader packetReader)
    {
        SC_MonsterPositionUpdatePacket packet = NetworkPacketUtil.PacketBytesToPacketObject<SC_MonsterPositionUpdatePacket>(packetReader);
        MonsterPosUpdates.Add(packet);
        MonsterPositionUpdate?.Invoke();
    }
    static void HandleSC_MonstersHealthUpdate(NetPacketReader dataReader)
    {
        SC_MonstersHealthUpdate packet = NetworkPacketUtil.PacketBytesToPacketObject<SC_MonstersHealthUpdate>(dataReader);
        MonstersHealthUpdateQueue.Add(packet);
        MonstersHealthUpdate?.Invoke();
    }

    static void Handle_SC_EquippedItemsUpdate(NetPacketReader dataReader)
    {
        Dictionary<EquipmentSlot, MongoInventoryItem> ei = new();
        SC_EquippedItemsUpdatePacket packet = NetworkPacketUtil.PacketBytesToPacketObject<SC_EquippedItemsUpdatePacket>(dataReader);
        for(int i = 0; i < packet.EquippedItems.Length; ++i)
        {
            var c = packet.EquippedItems[i];
            ei.Add(c.Slot, c.Item);
        }

        if (KnownEquippedItems.ContainsKey(packet.PublicId))
        {
            KnownEquippedItems[packet.PublicId] = ei;
        }
        else { 
            KnownEquippedItems.Add(packet.PublicId, ei);
        }

        EquippedItemsUpdate?.Invoke(packet.PublicId);
    }
    static void Handle_SC_CharacterAnimationUpdate(NetPacketReader dataReader)
    {
        SC_CharacterAnimationUpdatePacket packet = NetworkPacketUtil.PacketBytesToPacketObject<SC_CharacterAnimationUpdatePacket>(dataReader);
        if(packet.AnimationState == AnimationState.Start)
        {
            CharacterAnimationUpdate?.Invoke(packet.PublicId, (short)packet.AnimationType);
        }
    }
    static void Handle_SC_CharacterExpUpdatePacket(NetPacketReader dataReader)
    {
        SC_CharacterExpUpdatePacket packet = NetworkPacketUtil.PacketBytesToPacketObject<SC_CharacterExpUpdatePacket>(dataReader);
        KnownCharacterExp = (packet.Level, packet.Exp, packet.AvailableStatPoints);
        CharacterExpUpdate?.Invoke();
    }

    static void Handle_SC_CharacterLoggedOut(NetPacketReader dataReader)
    {
        SC_CharacterLoggedOutPacket packet = NetworkPacketUtil.PacketBytesToPacketObject<SC_CharacterLoggedOutPacket>(dataReader);
        CharacterLoggedOut?.Invoke(packet.PublicId);
    }
    static void Handle_SC_MonsterAnimationUpdate(NetPacketReader dataReader)
    {
        SC_MonsterAnimationUpdatePacket packet = NetworkPacketUtil.PacketBytesToPacketObject<SC_MonsterAnimationUpdatePacket>(dataReader);
        MonsterAnimationUpdates.Add(packet);
        MonsterAnimationUpdate?.Invoke();
    }
    static void Handle_SC_DamageHitsUpdate(NetPacketReader dataReader)
    {
        SC_DamageHitsUpdatePacket packet = NetworkPacketUtil.PacketBytesToPacketObject<SC_DamageHitsUpdatePacket>(dataReader);
        DamageHitsUpdates.Add(packet);
        DamageHitsUpdate?.Invoke();
    }
    static void Handle_SC_QuestsUpdate(NetPacketReader dataReader)
    {
        SC_QuestsUpdatePacket packet = NetworkPacketUtil.PacketBytesToPacketObject<SC_QuestsUpdatePacket>(dataReader);
        QuestProgressUpdates.Add(packet);
        QuestsProgressUpdate?.Invoke();
    }
    static void Handle_SC_NpcUpdate(NetPacketReader dataReader)
    {
        SC_NpcUpdatePacket packet = NetworkPacketUtil.PacketBytesToPacketObject<SC_NpcUpdatePacket>(dataReader);
        NpcUpdates.Add(packet);
        NpcUpdate?.Invoke();
    }
    static void Handle_SC_Mount(NetPacketReader dataReader)
    {
        SC_Mount packet = NetworkPacketUtil.PacketBytesToPacketObject<SC_Mount>(dataReader);
        NewestMountUpdate = packet;
        MountUpdate?.Invoke();
    }
}
