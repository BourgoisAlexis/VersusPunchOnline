using System;
using UnityEngine;

[Serializable]
public class Bonus {
    public string id;
    [Multiline(5)]
    public string description;
    public Sprite sprite;

    public void Use() {
        Utils.Log(this, $"{id}");
    }
}
