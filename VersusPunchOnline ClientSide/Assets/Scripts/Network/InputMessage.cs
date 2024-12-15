using System;
using System.Collections.Generic;

[Serializable]
public class InputMessage : PeerMessage {
    private const string TYPE = "Input";

    public int FrameIndex;
    public int PlayerIndex;
    public List<string> Inputs = new List<string>();


    public InputMessage() : base(TYPE) { }

    public InputMessage(int frameIndex, int playerIndex) : this(frameIndex, playerIndex, DateTime.Now.TimeOfDay) { }

    public InputMessage(int frameIndex, int playerIndex, TimeSpan time) : base(TYPE) {
        FrameIndex = frameIndex;
        PlayerIndex = playerIndex;
        Time = time.TotalMilliseconds;

        Inputs = new List<string>() {
            InputAction.Neutral.ToString()
        };
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
