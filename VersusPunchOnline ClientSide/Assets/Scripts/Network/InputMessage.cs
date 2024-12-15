using System;
using System.Collections.Generic;

[Serializable]
public class InputMessage : PeerMessage {
    public int FrameIndex;
    public int PlayerIndex;
    public List<string> Inputs = new List<string>();

    public InputMessage(int frameIndex, int playerIndex, TimeSpan time) : base("Input") {
        FrameIndex = frameIndex;
        PlayerIndex = playerIndex;
        Time = time.TotalMilliseconds;

        Inputs = new List<string>() {
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
        if (Inputs.Contains(input))
            return;

        Inputs.Add(input);
    }
}
