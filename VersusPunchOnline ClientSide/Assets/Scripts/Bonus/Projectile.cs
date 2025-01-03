using System;
using UnityEngine;

public class Projectile : Bonus {
    [Header("Projectile")]
    public float Speed;
    public float Gravity;
    public float Delay;
    public float Size;

    public override void Use() {
        Utils.Log(this, "Use", $"Projectile {Id}");
    }
}
