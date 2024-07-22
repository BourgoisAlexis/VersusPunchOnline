using UnityEngine;

namespace DPhysx {
    public class DPhysxCircle : DPhysxRigidbody {
        public float radius;

        public DPhysxCircle(FixedPoint2 center, float radius, Transform t = null, bool isStatic = false, bool isTrigger = false) : base(center, t, isStatic, isTrigger) {
            this.radius = radius;
        }
    }
}