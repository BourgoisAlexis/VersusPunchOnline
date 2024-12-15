using System;
using UnityEngine;

[Serializable]
public class Bonus {
    public string Id;
    [Multiline(5)]
    public string Description;
    public Sprite Sprite;

    public void Use() {
        Utils.Log(this, "Use", $"{Id}");
    }
}
