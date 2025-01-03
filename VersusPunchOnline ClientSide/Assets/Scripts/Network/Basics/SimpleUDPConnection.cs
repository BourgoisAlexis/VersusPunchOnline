public class SimpleUDPConnection<T> : UDPConnection<T> where T : PeerMessage {

    public override void SendMessage(object obj) {
        _udpGuest?.SendMessage(obj);
    }
}
