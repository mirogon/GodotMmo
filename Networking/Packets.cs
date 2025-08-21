
using System;
using System.Security.Cryptography;

public enum EPacketType
{
    Unknown = 0,
    CS_Register,
    SC_Register,
    CS_PositionUpdate,
    SC_PlayerUpdate,
}
public interface Packet
{
    public static int ByteSize => 0;
    public EPacketType PacketType => EPacketType.Unknown;
    public byte[] ToByteArray();
}

public class CS_RegisterPacket : Packet
{
    public static int ByteSize => 2;
    public EPacketType PacketType => EPacketType.CS_Register;
    public byte Empty;
    public CS_RegisterPacket() { }
    public CS_RegisterPacket(byte[] data) {}
    public byte[] ToByteArray()
    {
        byte[] bytes = new byte[2];
        bytes[0] = (byte)EPacketType.CS_Register;
        bytes[1] = 0;
        return bytes;
    }
}

public class SC_RegisterPacket : Packet
{
    public static int ByteSize => 9;
    public EPacketType PacketType => EPacketType.SC_Register;
    public UInt64 Id;
    public SC_RegisterPacket(UInt64 id)
    {
        Id = id;
    }
    public SC_RegisterPacket(byte[] data)
    {
        Id = BitConverter.ToUInt64(data, 1);
    }
    public byte[] ToByteArray()
    {
        byte[] data = new byte[9];
        data[0] = (byte)EPacketType.SC_Register;
        BitConverter.GetBytes(Id).CopyTo(data, 1);
        return data;
    }
}

public class CS_PositionUpdate : Packet
{
    public static int ByteSize => 21;
    public EPacketType PacketType => EPacketType.CS_PositionUpdate;
    public UInt64 SessionId;
    public float X;
    public float Y;
    public float Z;
    public CS_PositionUpdate(UInt64 sessionId, float x, float y, float z)
    {
        SessionId = sessionId;
        X = x;
        Y = y;
        Z = z;
    }
    public CS_PositionUpdate(byte[] data)
    {
        SessionId = BitConverter.ToUInt64(data, 1);
        X = BitConverter.ToSingle(data, 9);
        Y = BitConverter.ToSingle(data, 13);
        Z = BitConverter.ToSingle(data, 17);
    }
    public byte[] ToByteArray()
    {
        byte[] bytes = new byte[21];
        bytes[0] = (byte)EPacketType.CS_PositionUpdate;
        BitConverter.GetBytes(SessionId).CopyTo(bytes, 1);
        BitConverter.GetBytes(X).CopyTo(bytes, 9);
        BitConverter.GetBytes(Y).CopyTo(bytes, 13);
        BitConverter.GetBytes(Z).CopyTo(bytes, 17);
        return bytes;
    }
}
public class SC_PlayerUpdatePacket : Packet
{
    public static int ByteSize => 21;
    public EPacketType PacketType => EPacketType.SC_PlayerUpdate;
    public UInt64 PublicId;
    public float X;
    public float Y;
    public float Z;

    public SC_PlayerUpdatePacket(UInt64 sessionId, float x, float y, float z)
    {
        PublicId = sessionId;
        X = x;
        Y = y;
        Z = z;
    }
    public SC_PlayerUpdatePacket(byte[] data)
    {
        PublicId = BitConverter.ToUInt64(data, 1);
        X = BitConverter.ToSingle(data, 9);
        Y = BitConverter.ToSingle(data, 13);
        Z = BitConverter.ToSingle(data, 17);
    }

    public byte[] ToByteArray()
    {
        byte[] bytes = new byte[21];
        bytes[0] = (byte)EPacketType.SC_PlayerUpdate;
        BitConverter.GetBytes(PublicId).CopyTo(bytes, 1);
        BitConverter.GetBytes(X).CopyTo(bytes, 9);
        BitConverter.GetBytes(Y).CopyTo(bytes, 13);
        BitConverter.GetBytes(Z).CopyTo(bytes, 17);

        return bytes;
    }

}
