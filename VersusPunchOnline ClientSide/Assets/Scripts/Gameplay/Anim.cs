using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AnimHitBox {
    public int frame;
    public Vector2 position;
    public Vector2 size;
}

[CreateAssetMenu(menuName = "Anims")]
public class Anim : ScriptableObject {
    public AnimationClip clip;
    public List<AnimHitBox> hitBoxes;
}
