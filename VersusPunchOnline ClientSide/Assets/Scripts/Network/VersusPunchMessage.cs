using UnityEngine;

public abstract class VersusPunchMessage {
    public string type;
    public double time;

    public override string ToString() {
        return JsonUtility.ToJson(this);
    }
}
