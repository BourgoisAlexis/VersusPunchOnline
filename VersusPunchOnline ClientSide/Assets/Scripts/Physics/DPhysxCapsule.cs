using System.Collections.Generic;
using UnityEngine;

namespace DPhysx {
    public class DPhysxCapsule : DPhysxShape {
        public float height;
        public float width;

        public DPhysxCapsule(FixedPoint2 center, float height, float width, Transform t = null, bool isStatic = false, bool isTrigger = false) : base(center, t, isStatic, isTrigger) {
            this.height = height;
            this.width = width;
        }

        public List<DPhysxCircle> GetCollisions() {
            List<DPhysxCircle> circles = new List<DPhysxCircle>();

            //for (int i = -1; i < 2; i += 2) {
            //    float radius = width / 2;
            //    float margin = height / 2 - radius;
            //    Vector2 pp = center + Vector2.up * (i * margin);
            //    circles.Add(new DPhysxCircle(pp, isStatic, radius));
            //}

            return circles;
        }
    }
}
