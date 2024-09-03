using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FrameInfo {
    public int index;
    public List<List<string>> inputs = new List<List<string>>();
    public double time; //used for ping

    public FrameInfo(int index, TimeSpan time) {
        this.index = index;
        this.time = time.TotalMilliseconds;

        inputs = new List<List<string>>() {
            new List<string>{ InputAction.Neutral.ToString() },
            new List<string>{ InputAction.Neutral.ToString() },
        };
    }

    public FrameInfo(int index) : this(index, DateTime.Now.TimeOfDay) {

    }

    public void AddInput(InputAction input, int playerIndex) {
        string s = input.ToString();

        if (inputs[playerIndex].Contains(s))
            return;

        inputs[playerIndex].Add(s);
    }

    public override string ToString() {
        return JsonUtility.ToJson(this);
    }

    public static FrameInfo FromString(string s) {
        return JsonUtility.FromJson<FrameInfo>(s);
    }
}
