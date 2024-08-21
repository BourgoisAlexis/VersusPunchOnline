using System.Collections.Generic;
using UnityEngine;

namespace DPhysx {
    public class DPhysxManager : MonoBehaviour {
        #region Variables
        private FixedPoint _coefficientOfRestitution = FP.fp(0.1f);
        private FixedPoint _kineticFriction = FP.fp(0.01f);

        private FixedPoint _gravity = FP.fp(0.06f);
        private FixedPoint _minimalVelocity = FP.fp(0.003f);
        private int _itterations = 16;

        private int _currentIndex = 0;
        private List<DPhysxRigidbody> _rbs = new List<DPhysxRigidbody>();
        private List<DPhysxRigidbody> _rbsToRemove = new List<DPhysxRigidbody>();
        private Dictionary<DPhysxRigidbody, int> _temporaryRBs = new Dictionary<DPhysxRigidbody, int>();
        private Dictionary<DPhysxRigidbody, List<DPhysxRigidbody>> _triggers = new Dictionary<DPhysxRigidbody, List<DPhysxRigidbody>>();

        private int _boxSize = 1;
        private DPhysxSpatialGrid _spatialGrid = new DPhysxSpatialGrid(1);
        #endregion


        public void Setup() {
            GlobalManager.Instance.onCustomUpdate += Simulate;
            GlobalManager.Instance.NavigationManager.onLoadScene += ClearRigidbodies;
        }

        private void Simulate() {
            UpdateRemovedRigidbodies();

            List<DPhysxRigidbody> rbs = new List<DPhysxRigidbody>(_rbs);

            foreach (DPhysxRigidbody rb in rbs) {
                UpdateTemporaryRigidbodies(rb);

                if (rb.isStatic)
                    continue;

                rb.velocity -= rb.velocity.normalized * _kineticFriction;
                rb.velocity += rb.acceleration;
                rb.velocity += new FixedPoint2(FixedPoint.zero, -_gravity);
            }

            for (int x = 0; x < _itterations; x++) {
                foreach (DPhysxBox box1 in _rbs) {
                    foreach (DPhysxBox box2 in _spatialGrid.GetBoxes(box1)) {
                        if (box1 != box2) {
                            if (SquareToSquare(box1, box2)) {
                                if (!box1.isTrigger && !box2.isTrigger)
                                    ResolveCollision(box1, box2);

                                if (box1.isTrigger)
                                    TriggerEnter(box1, box2);
                                if (box2.isTrigger)
                                    TriggerEnter(box2, box1);
                            }
                            else {
                                if (box1.isTrigger)
                                    TriggerExit(box1, box2);
                                if (box2.isTrigger)
                                    TriggerExit(box2, box1);
                            }
                        }
                    }
                }

                foreach (DPhysxRigidbody rb in rbs) {
                    DPhysxBox old = new DPhysxBox(rb as DPhysxBox);
                    old.id = rb.id;

                    rb.center += rb.velocity / FP.fp(_itterations);
                    if (rb.t != null)
                        rb.t.position = rb.center.ToVector3();

                    _spatialGrid.Update(old, rb as DPhysxBox);
                }
            }
        }


        private bool SquareToSquare(DPhysxBox box1, DPhysxBox box2) {
            if (box1.max.x < box2.min.x || box2.max.x < box1.min.x)
                return false;

            if (box1.max.y < box2.min.y || box2.max.y < box1.min.y)
                return false;

            return true;
        }

        public List<DPhysxRigidbody> BoxCast(FixedPoint2 origin, FixedPoint2 end, int selfID) {
            List<DPhysxRigidbody> result = new List<DPhysxRigidbody>();
            FixedPoint2 direction = end - origin;
            FixedPoint2 center = origin + (direction / FP.fp(2f));

            DPhysxBox box = new DPhysxBox(center, new FixedPoint2(FP.Abs(direction.x), FP.Abs(direction.y)), null, true, true);
            box.id = selfID;

            Debug.DrawLine(origin.ToVector2(), end.ToVector2(), Color.yellow, 0.1f);

            foreach (DPhysxBox b in _rbs)
                if (b.id != selfID)
                    if (SquareToSquare(box, b))
                        result.Add(b);

            return result;
        }

        private void ResolveCollision(DPhysxBox box1, DPhysxBox box2) {
            FixedPoint2 pene = new FixedPoint2(GetPeneX(box1, box2), GetPeneY(box1, box2));
            bool resolveOnX = FP.Abs(pene.x) <= FP.Abs(pene.y);

            pene = resolveOnX ? new FixedPoint2(pene.x, FixedPoint.zero) : new FixedPoint2(FixedPoint.zero, pene.y);

            FixedPoint2 v = pene / (box1.isStatic ? FP.fp(1) : FP.fp(2));

            if (!box1.isStatic)
                box1.center -= pene / (box2.isStatic ? FP.fp(1) : FP.fp(2));
            if (!box2.isStatic)
                box2.center += pene / (box1.isStatic ? FP.fp(1) : FP.fp(2));

            FixedPoint2 relativeVelocity = box2.velocity - box1.velocity;
            FixedPoint2 normal = resolveOnX ? new FixedPoint2(FP.Sign(pene.x), FixedPoint.zero) : new FixedPoint2(FixedPoint.zero, FP.Sign(pene.y));
            FixedPoint velocityAlongNormal = FixedPoint2.Dot(relativeVelocity, normal);

            if (velocityAlongNormal > FixedPoint.zero)
                return;

            FixedPoint combinedRestitution = _coefficientOfRestitution + _coefficientOfRestitution / FP.fp(2);
            FixedPoint impulseMagnitude = -(FP.fp(1) + combinedRestitution) * velocityAlongNormal;

            FixedPoint2 impulse = normal * impulseMagnitude;


            if (!box1.isStatic)
                box1.velocity -= impulse;
            if (!box2.isStatic)
                box2.velocity += impulse;

            box1.velocity = AdjustValues(box1.velocity);
            box2.velocity = AdjustValues(box2.velocity);
        }

        private void TriggerEnter(DPhysxBox box1, DPhysxBox box2) {
            if (!_triggers.ContainsKey(box1)) {
                _triggers.Add(box1, new List<DPhysxRigidbody> { box2 });
                box1.onTriggerEnter?.Invoke(box2);
            }
            else if (!_triggers[box1].Contains(box2)) {
                _triggers[box1].Add(box2);
                box1.onTriggerEnter?.Invoke(box2);
            }
            else {
                Utils.Log(this, "TriggerEnter", "Trigger Stay", true);
            }
        }

        private void TriggerExit(DPhysxRigidbody col1, DPhysxRigidbody col2) {
            if (!_triggers.ContainsKey(col1))
                return;

            if (!_triggers[col1].Contains(col2))
                return;

            col1.onTriggerExit?.Invoke(col2);
            _triggers[col1].Remove(col2);
        }

        private FixedPoint2 AdjustValues(FixedPoint2 v) {
            FixedPoint x = v.x * v.x;
            FixedPoint y = v.y * v.y;

            if (x <= _minimalVelocity)
                v.x = FixedPoint.zero;

            if (y <= _minimalVelocity)
                v.y = FixedPoint.zero;

            return v;
        }


        private FixedPoint GetPeneY(DPhysxBox col1, DPhysxBox col2) {
            FixedPoint2 difference = col1.center - col2.center;
            FixedPoint overlap = col1.height / FP.fp(2) + col2.height / FP.fp(2) - FP.Abs(difference.y);

            return overlap * (difference.y > FixedPoint.zero ? FP.fp(-1) : FP.fp(1));
        }

        private FixedPoint GetPeneX(DPhysxBox col1, DPhysxBox col2) {
            FixedPoint2 difference = col1.center - col2.center;
            FixedPoint overlap = col1.width / FP.fp(2) + col2.width / FP.fp(2) - FP.Abs(difference.x);

            return overlap * (difference.x > FixedPoint.zero ? FP.fp(-1) : FP.fp(1));
        }


        public void AddRigidbody(DPhysxRigidbody rigidbody, int duration = 0) {
            rigidbody.id = _currentIndex;
            _currentIndex++;
            _rbs.Add(rigidbody);

            if (rigidbody as DPhysxBox != null)
                _spatialGrid.AddBox(rigidbody as DPhysxBox);

            if (duration > 0)
                _temporaryRBs.Add(rigidbody, duration);
        }

        public void RemoveRigidbody(DPhysxRigidbody rigidbody) {
            _rbsToRemove.Add(rigidbody);
        }

        private void ClearRigidbodies() {
            _rbs.Clear();
            _temporaryRBs.Clear();
        }


        private void UpdateTemporaryRigidbodies(DPhysxRigidbody rigidbody) {
            if (!_temporaryRBs.ContainsKey(rigidbody))
                return;

            if (_temporaryRBs[rigidbody] < 1) {
                RemoveRigidbody(rigidbody);
                return;
            }

            _temporaryRBs[rigidbody]--;
        }

        private void UpdateRemovedRigidbodies() {
            foreach (DPhysxRigidbody rb in _rbsToRemove) {
                _rbs.Remove(rb);

                if (rb as DPhysxBox != null)
                    _spatialGrid.RemoveBox(rb as DPhysxBox);

                if (_temporaryRBs.ContainsKey(rb))
                    _temporaryRBs.Remove(rb);

                if (_triggers.ContainsKey(rb)) {
                    List<DPhysxRigidbody> inTrigger = new List<DPhysxRigidbody>(_triggers[rb]);

                    foreach (DPhysxRigidbody s in inTrigger) {
                        TriggerExit(rb, s);

                        if (s.isTrigger)
                            TriggerExit(s, rb);
                    }

                    _triggers.Remove(rb);
                }
            }

            _rbsToRemove.Clear();
        }


        private void OnDrawGizmos() {
            foreach (DPhysxRigidbody rb in _rbs) {
                Gizmos.color = rb.color;
                Vector2 center = rb.center.ToVector2();
                Vector2 centerVelocity = (rb.center + rb.velocity).ToVector2();

                if (!rb.isStatic) {
                    Gizmos.DrawLine(center, centerVelocity);
                    Gizmos.DrawLine(centerVelocity, centerVelocity + Vector2.left * 0.2f);
                }

                Gizmos.color = AppConst.RigidBodyColor(rb);

                DPhysxCircle circle = rb as DPhysxCircle;
                if (circle != null) {
                    Gizmos.DrawWireSphere(center, circle.radius);
                    continue;
                }

                DPhysxBox square = rb as DPhysxBox;
                if (square != null) {
                    Gizmos.DrawLine(square.max.ToVector3(), square.min.ToVector3());
                    Gizmos.DrawWireCube(center, new Vector3(square.width.ToFloat(), square.height.ToFloat(), 0));
                    continue;
                }
            }

            if (_spatialGrid == null)
                return;

            Gizmos.color = AppConst.SpatialGridColor;

            foreach (KeyValuePair<System.Numerics.Vector2, List<DPhysxRigidbody>> cell in _spatialGrid.Cells) {
                Gizmos.DrawWireCube(new Vector2(cell.Key.X, cell.Key.Y) * _boxSize + Vector2.one * _boxSize / 2, Vector2.one * _boxSize);
            }
        }
    }
}
