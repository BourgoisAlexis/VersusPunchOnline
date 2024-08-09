using DPhysx;
using UnityEngine;

public enum PlayerStates {
    Idle,
    Run,
    Midair,
    Punch
}

public class PlayerController : MonoBehaviour {
    #region Variables
    public int playerIndex;

    [SerializeField] private PlayerVisual _playerVisual;
    [SerializeField] private DPhysxRigidbodyMonobehaviour _rigidbodyMono;

    private FixedPoint _maxSpeed = FP.fp(0.4f);
    private FixedPoint _jumpHeight = FP.fp(8);
    private DPhysxBox _rb;
    private ControlBuffer _buffer;
    private bool _grounded;
    private FixedPoint _visualDirection;

    private PlayerStates _state;
    private int _blockState;
    private int _blockStateValue;
    #endregion


    public void Init() {
        _buffer = new ControlBuffer();
        _buffer.Init(5);
        _rb = _rigidbodyMono.rb as DPhysxBox;
        _rb.tags.Add(AppConst.tagPlayer);
        _blockState = 0;
    }

    public void ExecuteInputs(FrameData frame) {
        string json = JsonUtility.ToJson(frame);
        FixedPoint2 accel = new FixedPoint2(0, 0);

        _buffer.Update(frame);

        DetectGround();
        DetectWallLeft();
        DetectWallRight();
        Move(ref accel, out FixedPoint dirInput);
        Jump(ref accel);
        bool punch = Hit();

        if (_blockState > 0) {
            _blockState--;
        }
        else {
            if (punch)
                _state = PlayerStates.Punch;
            else if (_rb.velocity.y != FixedPoint.zero)
                _state = PlayerStates.Midair;
            else if (_rb.velocity.x != FixedPoint.zero)
                _state = PlayerStates.Run;
            else
                _state = PlayerStates.Idle;

            if (_blockStateValue > 0) {
                _blockState = _blockStateValue;
                _blockStateValue = 0;
            }
        }

        _rb.acceleration = accel * FP.fp(0.1f);
        _playerVisual.UpdateVisual(_state, _rb);
    }


    private void DetectGround() {
        FixedPoint2 start = new FixedPoint2(_rb.min.x, _rb.center.y);
        FixedPoint2 end = new FixedPoint2(_rb.max.x, _rb.min.y - FP.fp(0.2f));

        _grounded = GlobalManager.Instance.PhysicsManager.BoxCast(start, end, _rb.id).Count > 0;
    }

    private void DetectWallLeft() {
        FixedPoint2 start = new FixedPoint2(_rb.center.x, _rb.min.y);
        FixedPoint2 end = new FixedPoint2(_rb.max.x + FP.fp(0.2f), _rb.max.y);

        GlobalManager.Instance.PhysicsManager.BoxCast(start, end, _rb.id);
    }

    private void DetectWallRight() {
        FixedPoint2 start = new FixedPoint2(_rb.center.x, _rb.min.y);
        FixedPoint2 end = new FixedPoint2(_rb.min.x - FP.fp(0.2f), _rb.max.y);

        GlobalManager.Instance.PhysicsManager.BoxCast(start, end, _rb.id);
    }

    private void Move(ref FixedPoint2 accel, out FixedPoint input) {
        FixedPoint directionalInput = FixedPoint.zero;
        FixedPoint airbornRatio = FP.fp(0.5f);

        if (_buffer.GetBufferedInput(KeyCode.LeftArrow))
            directionalInput += FP.fp(-1);
        else if (_buffer.GetBufferedInput(KeyCode.RightArrow))
            directionalInput += FP.fp(1);

        if (directionalInput == FixedPoint.zero)
            accel -= new FixedPoint2(_rb.velocity.normalized.x * FP.fp(0.5f), FixedPoint.zero);
        else if (FP.Abs(_rb.velocity.x) < _maxSpeed)
            accel += new FixedPoint2(directionalInput * (_grounded ? FP.fp(1) : airbornRatio), FP.fp(0));

        if (directionalInput != FixedPoint.zero)
            _visualDirection = _rb.velocity.x < FixedPoint.zero ? FP.fp(-1) : FP.fp(1);

        input = directionalInput;
    }

    private void Jump(ref FixedPoint2 accel) {
        if (_grounded)
            if (_buffer.GetBufferedInput(KeyCode.UpArrow))
                accel += new FixedPoint2(FixedPoint.zero, _jumpHeight);
    }

    private bool Hit() {
        if (!_buffer.GetBufferedInput(KeyCode.Space))
            return false;

        DPhysxBox box = new DPhysxBox(_rb.center + new FixedPoint2(FP.fp(2) * _visualDirection, FixedPoint.zero), 1, 1, null, true, true);
        box.tags.Add(AppConst.tagHitBox);
        box.onTriggerEnter += PlayerHit;
        GlobalManager.Instance.PhysicsManager.AddRigidbody(box, 10);
        _blockStateValue = 15;
        return true;
    }

    private void PlayerHit(DPhysxRigidbody shape) {
        if (!shape.isStatic)
            shape.velocity += new FixedPoint2(0.5f, 1);
    }
}
