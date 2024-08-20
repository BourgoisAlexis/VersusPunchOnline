using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SnapShot {
    public int index;
    public List<string> inputs = new List<string>();
    public double time; //used for ping

    public SnapShot(int index, TimeSpan time) {
        this.index = index;
        this.time = time.TotalMilliseconds;

        inputs = new List<string>() { InputAction.Neutral.ToString() };
    }

    public SnapShot(int index) : this(index, DateTime.Now.TimeOfDay) { }

    public void AddInput(InputAction input) {
        string s = input.ToString();

        if (inputs.Contains(s))
            return;

        inputs.Add(s);
    }

    public override string ToString() {
        return JsonUtility.ToJson(this);
    }

    public static SnapShot FromString(string s) {
        return JsonUtility.FromJson<SnapShot>(s);
    }
}
