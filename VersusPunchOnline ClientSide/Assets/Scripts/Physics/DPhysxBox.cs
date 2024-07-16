using UnityEngine;

namespace DPhysx {
    public class DPhysxBox : DPhysxShape {
        public FixedPoint width;
        public FixedPoint height;

        public FixedPoint2 min => new FixedPoint2(center.x - width / FP.fp(2), center.y - height / FP.fp(2));
        public FixedPoint2 max => new FixedPoint2(center.x + width / FP.fp(2), center.y + height / FP.fp(2));

        public DPhysxBox(FixedPoint2 center, float width, float height, Transform t = null, bool isStatic = false, bool isTrigger = false) : base(center, t, isStatic, isTrigger) {
            this.width = FP.fp(width);
            this.height = FP.fp(height);
        }

        public DPhysxBox(DPhysxBox box) : base(box.center, box.t, box.isStatic, box.isTrigger) {
            this.width = box.width;
            this.height = box.height;
        }

        public DPhysxBox(FixedPoint2 center, FixedPoint2 size, Transform t = null, bool isStatic = false, bool isTrigger = false) : base(center, t, isStatic, isTrigger) {
            width = size.x;
            height = size.y;
        }
    }
}
