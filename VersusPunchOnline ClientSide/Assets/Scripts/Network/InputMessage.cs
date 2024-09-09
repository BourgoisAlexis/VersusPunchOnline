using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InputMessage : VersusPunchMessage {
    public int frameIndex;
    public int playerIndex;
    public List<string> inputs = new List<string>();

    public InputMessage(int frameIndex, int playerIndex, TimeSpan time) {
        this.type = "Input";
        this.frameIndex = frameIndex;
        this.playerIndex = playerIndex;
        this.time = time.TotalMilliseconds;

        inputs = new List<string>() {
            InputAction.Neutral.ToString()
        };
    }

    public InputMessage(int frameIndex, int playerIndex) : this(frameIndex, playerIndex, DateTime.Now.TimeOfDay) {

    }

    public void AddInput(InputAction input, int playerIndex) {
        string s = input.ToString();
        AddInput(s);
    }

    public void AddInput(string input) {
        if (inputs.Contains(input))
            return;

        inputs.Add(input);
    }

    public static InputMessage FromString(string s) {
        return JsonUtility.FromJson<InputMessage>(s);
    }
}
