using DPhysx;
using UnityEngine;

public enum ShapeType {
    Box,
    Circle,
    None,
}

public class DPhysxRigidbodyMonobehaviour : MonoBehaviour {
    [SerializeField] private ShapeType _shapeType;
    [SerializeField] private Vector2 _size;
    [SerializeField] private bool _isStatic;
    [SerializeField] private bool _isTrigger;

    public DPhysxRigidbody rb = null;


    private void Start() {
        FixedPoint2 center = FixedPoint2.FromVector2(transform.position);

        if (_shapeType == ShapeType.Circle)
            rb = new DPhysxCircle(center, _size.x, transform, _isStatic, _isTrigger);
        else if (_shapeType == ShapeType.Box)
            rb = new DPhysxBox(center, _size.x, _size.y, transform, _isStatic, _isTrigger);

        GlobalManager.Instance.PhysicsManager.AddRigidbody(rb);
    }

    private void OnDrawGizmos() {
        if (Application.isPlaying)
            return;

        Gizmos.color = _isTrigger ? Color.white : (_isStatic ? Color.red : Color.yellow);

        if (_shapeType == ShapeType.Circle) {
            Gizmos.DrawWireSphere(transform.position, _size.x);
            return;
        }

        if (_shapeType == ShapeType.Box) {
            Gizmos.DrawWireCube(transform.position, new Vector3(_size.x, _size.y, 0));
            return;
        }
    }
}
