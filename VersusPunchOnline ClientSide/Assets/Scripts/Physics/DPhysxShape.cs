using System;
using System.Collections.Generic;
using UnityEngine;

namespace DPhysx {
    [Serializable]
    public class DPhysxShape {
        public int id = -1;
        public List<string> tags = new List<string>();

        public FixedPoint2 center;
        public FixedPoint2 velocity;
        public FixedPoint2 acceleration;
        public Transform t;
        public bool isStatic = false;
        public bool isTrigger = false;

        public Color color;

        public Action<DPhysxShape> onTriggerEnter;
        public Action<DPhysxShape> onTriggerExit;

        public DPhysxShape(FixedPoint2 center, Transform t = null, bool isStatic = false, bool isTrigger = false) {
            color = Utils.RandomColor();

            this.center = center;
            this.isStatic = isStatic;
            this.t = t;
            this.isTrigger = isTrigger;
        }
    }
}
