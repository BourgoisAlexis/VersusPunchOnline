using Newtonsoft.Json;
using System;

[Serializable]
public class SimpleMessage {
    public string Message;
    public double Time;


    public SimpleMessage(string message) {
        Message = message;
        Time = DateTime.Now.TimeOfDay.TotalMilliseconds;
    }


    public static SimpleMessage FromString(string s) {
        return JsonConvert.DeserializeObject<SimpleMessage>(s);
    }

    public override string ToString() {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}
