using System.Collections.Generic;
using System.Text;
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
        private List<DPhysxShape> _shapes = new List<DPhysxShape>();
        private List<DPhysxShape> _shapesToRemove = new List<DPhysxShape>();
        private Dictionary<DPhysxShape, int> _temporaryShapes = new Dictionary<DPhysxShape, int>();
        private Dictionary<DPhysxShape, List<DPhysxShape>> _triggers = new Dictionary<DPhysxShape, List<DPhysxShape>>();

        private int _boxSize = 1;
        private DPhysxSpatialGrid _spatialGrid = new DPhysxSpatialGrid(1);
        #endregion


        public void Setup() {
            GlobalManager.Instance.onCustomUpdate += Simulate;
            GlobalManager.Instance.navigationManager.onLoadScene += ClearShapes;
        }

        private void Simulate() {
            UpdateRemovedShapes();

            List<DPhysxShape> shapes = new List<DPhysxShape>(_shapes);

            foreach (DPhysxShape shape in shapes) {
                UpdateTemporaryShape(shape);

                if (shape.isStatic)
                    continue;

                shape.velocity -= shape.velocity.normalized * _kineticFriction;
                shape.velocity += shape.acceleration;
                shape.velocity += new FixedPoint2(FixedPoint.zero, -_gravity);
            }

            for (int x = 0; x < _itterations; x++) {
                foreach (DPhysxBox b1 in _shapes) {
                    foreach (DPhysxBox b2 in _spatialGrid.GetBoxes(b1)) {
                        if (b1 != b2) {
                            if (SquareToSquare(b1, b2)) {
                                if (!b1.isTrigger && !b2.isTrigger)
                                    ResolveCollision(b1, b2);

                                if (b1.isTrigger)
                                    TriggerEnter(b1, b2);
                                if (b2.isTrigger)
                                    TriggerEnter(b2, b1);
                            }
                            else {
                                if (b1.isTrigger)
                                    TriggerExit(b1, b2);
                                if (b2.isTrigger)
                                    TriggerExit(b2, b1);
                            }
                        }
                    }
                }

                foreach (DPhysxShape shape in shapes) {
                    DPhysxBox old = new DPhysxBox(shape as DPhysxBox);
                    old.id = shape.id;

                    shape.center += shape.velocity / FP.fp(_itterations);
                    if (shape.t != null)
                        shape.t.position = shape.center.ToVector3();

                    _spatialGrid.Update(old, shape as DPhysxBox);
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

        public List<DPhysxShape> BoxCast(FixedPoint2 origin, FixedPoint2 end, int selfID) {
            List<DPhysxShape> result = new List<DPhysxShape>();
            FixedPoint2 direction = end - origin;
            FixedPoint2 center = origin + (direction / FP.fp(2f));

            DPhysxBox box = new DPhysxBox(center, new FixedPoint2(FP.Abs(direction.x), FP.Abs(direction.y)), null, true, true);
            box.id = selfID;

            Debug.DrawLine(origin.ToVector2(), end.ToVector2(), Color.yellow, 0.1f);

            foreach (DPhysxBox b in _shapes)
                if (b.id != selfID)
                    if (SquareToSquare(box, b))
                        result.Add(b);

            return result;
        }

        private void ResolveCollision(DPhysxBox col1, DPhysxBox col2) {
            FixedPoint2 pene = new FixedPoint2(GetPeneX(col1, col2), GetPeneY(col1, col2));
            bool resolveOnX = FP.Abs(pene.x) <= FP.Abs(pene.y);

            pene = resolveOnX ? new FixedPoint2(pene.x, FixedPoint.zero) : new FixedPoint2(FixedPoint.zero, pene.y);

            FixedPoint2 v = pene / (col1.isStatic ? FP.fp(1) : FP.fp(2));

            if (!col1.isStatic)
                col1.center -= pene / (col2.isStatic ? FP.fp(1) : FP.fp(2));
            if (!col2.isStatic)
                col2.center += pene / (col1.isStatic ? FP.fp(1) : FP.fp(2));

            FixedPoint2 relativeVelocity = col2.velocity - col1.velocity;
            FixedPoint2 normal = resolveOnX ? new FixedPoint2(FP.Sign(pene.x), FixedPoint.zero) : new FixedPoint2(FixedPoint.zero, FP.Sign(pene.y));
            FixedPoint velocityAlongNormal = FixedPoint2.Dot(relativeVelocity, normal);

            if (velocityAlongNormal > FixedPoint.zero)
                return;

            FixedPoint combinedRestitution = _coefficientOfRestitution + _coefficientOfRestitution / FP.fp(2);
            FixedPoint impulseMagnitude = -(FP.fp(1) + combinedRestitution) * velocityAlongNormal;

            FixedPoint2 impulse = normal * impulseMagnitude;


            if (!col1.isStatic)
                col1.velocity -= impulse;
            if (!col2.isStatic)
                col2.velocity += impulse;

            col1.velocity = AdjustValues(col1.velocity);
            col2.velocity = AdjustValues(col2.velocity);
        }

        private void TriggerEnter(DPhysxBox col1, DPhysxBox col2) {
            if (!_triggers.ContainsKey(col1)) {
                _triggers.Add(col1, new List<DPhysxShape> { col2 });
                col1.onTriggerEnter?.Invoke(col2);
            }
            else if (!_triggers[col1].Contains(col2)) {
                _triggers[col1].Add(col2);
                col1.onTriggerEnter?.Invoke(col2);
            }
            else {
                //Utils.Log(this, "TriggerEnter", "Trigger Stay", true);
            }
        }

        private void TriggerExit(DPhysxShape col1, DPhysxShape col2) {
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


        public void AddShape(DPhysxShape shape, int duration = 0) {
            shape.id = _currentIndex;
            _currentIndex++;
            _shapes.Add(shape);

            Debug.Log($"{shape.velocity} {shape.id}");

            if (shape as DPhysxBox != null)
                _spatialGrid.AddBox(shape as DPhysxBox);

            if (duration > 0)
                _temporaryShapes.Add(shape, duration);
        }

        public void RemoveShape(DPhysxShape shape) {
            _shapesToRemove.Add(shape);

            if (shape as DPhysxBox != null)
                _spatialGrid.RemoveBox(shape as DPhysxBox);
        }

        private void ClearShapes() {
            _shapes.Clear();
            _temporaryShapes.Clear();
        }


        private void UpdateTemporaryShape(DPhysxShape shape) {
            if (!_temporaryShapes.ContainsKey(shape))
                return;

            if (_temporaryShapes[shape] < 1) {
                RemoveShape(shape);
                return;
            }

            _temporaryShapes[shape]--;
        }

        private void UpdateRemovedShapes() {
            foreach (DPhysxShape shape in _shapesToRemove) {
                _shapes.Remove(shape);

                if (_temporaryShapes.ContainsKey(shape))
                    _temporaryShapes.Remove(shape);

                if (_triggers.ContainsKey(shape)) {
                    List<DPhysxShape> inTrigger = new List<DPhysxShape>(_triggers[shape]);

                    foreach (DPhysxShape s in inTrigger) {
                        TriggerExit(shape, s);

                        if (s.isTrigger)
                            TriggerExit(s, shape);
                    }

                    _triggers.Remove(shape);
                }
            }

            _shapesToRemove.Clear();
        }


        private void OnDrawGizmos() {
            foreach (DPhysxShape col in _shapes) {
                Gizmos.color = col.color;
                Vector2 center = col.center.ToVector2();
                Vector2 centerVelocity = (col.center + col.velocity).ToVector2();

                if (!col.isStatic) {
                    Gizmos.DrawLine(center, centerVelocity);
                    Gizmos.DrawLine(centerVelocity, centerVelocity + Vector2.left * 0.2f);
                }

                Gizmos.color = col.isTrigger ? Color.white : (col.isStatic ? Color.red : col.color);

                DPhysxCircle circle = col as DPhysxCircle;
                if (circle != null) {
                    Gizmos.DrawWireSphere(center, circle.radius);
                    continue;
                }

                DPhysxBox square = col as DPhysxBox;
                if (square != null) {
                    Gizmos.DrawLine(square.max.ToVector3(), square.min.ToVector3());
                    Gizmos.DrawWireCube(center, new Vector3(square.width.ToFloat(), square.height.ToFloat(), 0));
                    continue;
                }
            }

            if (_spatialGrid == null)
                return;

            foreach (KeyValuePair<System.Numerics.Vector2, List<DPhysxShape>> cell in _spatialGrid.Cells) {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(new Vector2(cell.Key.X, cell.Key.Y) * _boxSize, Vector2.one * _boxSize);
            }
        }
    }
}
