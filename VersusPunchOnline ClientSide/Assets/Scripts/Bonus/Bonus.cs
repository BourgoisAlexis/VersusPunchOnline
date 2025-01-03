using System;
using UnityEngine;

[Serializable]
public class Bonus {
    public string Id;
    public string Description;
    public Sprite Sprite;

    public virtual void Use() {
        Utils.Log(this, "Use", $"{Id}");
    }
}
