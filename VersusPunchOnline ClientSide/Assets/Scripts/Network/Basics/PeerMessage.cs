using Newtonsoft.Json;
using System;

[Serializable]
public class PeerMessage {
    public string Message;
    public double Time;


    public PeerMessage(string message) {
        Message = message;
        Time = DateTime.Now.TimeOfDay.TotalMilliseconds;
    }


    public static T FromString<T>(string s) {
        return JsonConvert.DeserializeObject<T>(s);
    }

    public override string ToString() {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}
