# VersusPunch Online
VersusPunch was originaly created with some friends of mine during a game jam a few years ago (2019).  
The idea is to remake this game we really liked to create and make it so that it can be played online.
<br/>
<br/>

### Peer to peer connection
This is pretty much the first time i implemented peer to peer in a game working in Unity. Even though this is probably not a perfect implementation i'm happy with the result at this point.<br/>
I went with TCP connection first. But even if it was reliable, it was also too slow for a real time game. This would be a good solution for asynchronous game.<br/>
I then went with UDP connection using [LiteNetLib](https://github.com/RevenantX/LiteNetLib). This came out super well being stable and fast with pretty much 2 to 3 frames of lag in my tests.
<br/>
<br/>

### Custom physics
I wanted to implement custom physics to have the possibility to create a deterministic physics since Unity's physics is not.
After a few researches i implemented a simple 2D bounding box physics with FixedPoint values.<br/>
> cf : [Physics](/VersusPunchOnline%20ClientSide/Assets/Scripts/Physics)
<br/>

### Rollback netcode
Not implemented yet but I'm working on it.
