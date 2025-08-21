# WIP

# Later
- Server list of clients (ID, IP, Name)

# DONE
- Basic movement (copy from M2Like)
- MMO Camera (copy from M2Like)
- Use LiteNetLib as my reliable UDP networking library
- Use MessagePack library to serialize/deserialize packet instances
    - Use NuGetForUnity, 
- Use ConcurrentQueue to push new packets, get packets in update or in some kind of loop/repeated invoke in the unity main thread
- Register Packet
- Log packet send sizes
- Redo all packets custom myself without MessagePack
- Try using Godot instead of Unity and see how it goes
- Two clients can connect at the same time
- Every game client has a public id
- Replicate two client positions