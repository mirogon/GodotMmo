# Current Microtask
- [Client] Sends CreateCharacter packet to GameServer when clicking Create in the CreateNewCharacterScene

# Current Macrotask
- Create Character

# TODO
- Shared.cs in the GameServer project, that others also use for things like ECharacterClass enum, etc. that both need
- Download metin2 p server to check how the char creation screen works in the game
- Create metin like char screen with 4 rotating bases (4 can be changed by changing a single number)
- [GameServer] Keeps track of which player is successfully logged in with its session id, etc.
- GameServerLogin: 
    1. Player sends login packet to Gameserver with email, sessionId
    2. GameServer checks if email and sessionId match, registers players connection if it does
- Create char: Client sends create char packet -> GameServer creates char, creates mongodb entry -> GameServer sends result -> Client
- [GameServer] Character type
- Show when signup was successful or failed
- Godot Client Signup and Login
- When login succeeds, it switches to the main scene
- Server list of clients (ID, IP, Name)

# DONE
- Login: Client Sends Login HTTP message -> LoginServer sends success with session id -> Client
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
- [Client] Registering opens info window to show status
- [Client] Game starts with login screen, can switch to login scene
- [Client] Client can register, can log in, when he logs in, the scene switches to the main scene
    - LoginClient class has events with signup/login results that others can sub to
- [LoginServer] validate email, username, pw when registering
- Game Server checks if session id is correct, etc.
- LoginServer uses UInt64 for sessionId
- MmoServer UInt64 as SessionId type
- [GameServer] SC_RegisterPacket contains info about successful login or not, etc.
- Rename LoginServer to AccountServer
- CS_CreateCharacter packet type (PacketType, SessionId, CharacterClass)
- Create Empty Create Character Scene
- Arrow to rotate char slot base and select character slot
- GameServer handle CS_CreateCharacterPacket function