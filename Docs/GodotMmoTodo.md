# WIP
- [Client] Registering opens info window to show status
- [Client] Client can register, can log in, when he logs in, the scene switches to the main scene
    - LoginClient class has events with signup/login results that others can sub to

# Later
- Show when signup was successful or failed
- Godot Client Signup and Login
- When login succeeds, it switches to the main scene
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
- Learn more about godot and write doc about it
- Replicate the MMOCamera I wrote in Unity
- Move dir is mesh forward
- Replicate rotation
- Create Local MongoDB Server
    - DB for Game
    - DB for Account
- Have a working HTTPS server that insomnia can interact with
- Create HTTP Login Server
    - You need a username, valid email, password, but dont need to confirm the email
        - Instead, save the mac addr and maybe some other info to limit accs per mac addr to 5 or so
- InfoWindow Scene
- Find out how to debug the godot client code
    - Attach to process manually
