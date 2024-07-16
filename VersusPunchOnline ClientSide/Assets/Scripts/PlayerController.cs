using DPhysx;
using UnityEngine;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour {
    #region Variables
    [SerializeField] private Transform _visual;

    private FixedPoint _maxSpeed = FP.fp(0.4f);
    private FixedPoint _jumpHeight = FP.fp(8);
    private DPhysxBox _shape;
    private FixedPoint _visualDirection;
    private ControlBuffer _buffer;
    private bool _grounded;
    #endregion


    public void Setup() {
        _buffer = new ControlBuffer(5);
        _buffer.Setup();
        _shape = GetComponent<DPhysxShapeMonobehaviour>().shape as DPhysxBox;
        _shape.tags.Add(AppConst.tagPlayer);
    }

    public void ExecuteInputs(FrameData frame) {
        string json = JsonUtility.ToJson(frame);
        FixedPoint2 accel = new FixedPoint2(0, 0);

        _buffer.Update(frame);

        DetectGround();
        DetectWallLeft();
        DetectWallRight();
        Move(ref accel);
        //TODO : Completely decouple the visual
        Jump(ref accel);
        Hit();

        _shape.acceleration = accel * FP.fp(0.1f);
    }


    private void DetectGround() {
        FixedPoint2 start = new FixedPoint2(_shape.min.x, _shape.center.y);
        FixedPoint2 end = new FixedPoint2(_shape.max.x, _shape.min.y - FP.fp(0.2f));

        _grounded = GlobalManager.Instance.dPhysxManager.BoxCast(start, end, _shape.id).Count > 0;
    }

    private void DetectWallLeft() {
        FixedPoint2 start = new FixedPoint2(_shape.center.x, _shape.min.y);
        FixedPoint2 end = new FixedPoint2(_shape.max.x + FP.fp(0.2f), _shape.max.y);

        GlobalManager.Instance.dPhysxManager.BoxCast(start, end, _shape.id);
    }

    private void DetectWallRight() {
        FixedPoint2 start = new FixedPoint2(_shape.center.x, _shape.min.y);
        FixedPoint2 end = new FixedPoint2(_shape.min.x - FP.fp(0.2f), _shape.max.y);

        GlobalManager.Instance.dPhysxManager.BoxCast(start, end, _shape.id);
    }

    private void Move(ref FixedPoint2 accel) {
        FixedPoint directionalInput = FixedPoint.zero;
        FixedPoint airbornRatio = FP.fp(0.5f);

        if (_buffer.GetBufferedInput(KeyCode.LeftArrow))
            directionalInput += FP.fp(-1);
        else if (_buffer.GetBufferedInput(KeyCode.RightArrow))
            directionalInput += FP.fp(1);

        if (directionalInput == FixedPoint.zero)
            accel -= new FixedPoint2(_shape.velocity.normalized.x * FP.fp(0.5f), FixedPoint.zero);
        else if (FP.Abs(_shape.velocity.x) < _maxSpeed)
            accel += new FixedPoint2(directionalInput * (_grounded ? FP.fp(1) : airbornRatio), FP.fp(0));

        //Visual
        if (directionalInput != FixedPoint.zero) {
            if (FP.Sign(_shape.velocity.x) == FP.Sign(directionalInput))
                return;

            bool goingLeft = directionalInput < FixedPoint.zero;
            _visual.localEulerAngles = new Vector2(0, 180 * (goingLeft ? 1 : 0));
            _visualDirection = goingLeft ? FP.fp(-1) : FP.fp(1);
        }
    }

    private void Jump(ref FixedPoint2 accel) {
        if (_grounded)
            if (_buffer.GetBufferedInput(KeyCode.UpArrow))
                accel += new FixedPoint2(FixedPoint.zero, _jumpHeight);
    }

    private void Hit() {
        if (!_buffer.GetBufferedInput(KeyCode.Space))
            return;

        DPhysxBox box = new DPhysxBox(_shape.center + new FixedPoint2(FP.fp(2) * _visualDirection, FixedPoint.zero), 1, 1, null, true, true);
        box.tags.Add(AppConst.tagHitBox);
        box.onTriggerEnter += PlayerHit;
        GlobalManager.Instance.dPhysxManager.AddShape(box, 10);
    }

    private void PlayerHit(DPhysxShape shape) {
        if (!shape.isStatic)
            shape.velocity += new FixedPoint2(0.5f, 1);
    }
}
