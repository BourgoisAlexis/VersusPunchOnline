using DPhysx;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IInputUser {
    #region Variables
    [SerializeField] private PlayerVisual _playerVisual;
    [SerializeField] private DPhysxRigidbodyMonobehaviour _rigidbodyMono;

    private FixedPoint _maxSpeed = FP.fp(0.25f);
    private FixedPoint _jumpHeight = FP.fp(7);
    private int _bufferValue = 6;
    private int _coyote;

    private DPhysxBox _rb;
    private ControlBuffer _buffer;
    private bool _grounded;
    private int _playerIndex;
    private FixedPoint _visualDirection;
    private IInputUser _view;

    private PlayerStates _state;
    private int _freezePlayerState;
    private int _freezePlayerStateDuration;
    private PlayerBonus _bonus;
    #endregion


    public void Init(int playerIndex, FixedPoint2 position) {
        _buffer = new ControlBuffer();
        _buffer.Init(_bufferValue);
        _rb = _rigidbodyMono.rb as DPhysxBox;
        _rb.tags.Add(AppConst.TAGP_LAYER);
        _rb.center = position;
        _state = PlayerStates.Idle;
        _freezePlayerState = 0;
        _playerIndex = playerIndex;
        _bonus = new PlayerBonus();
    }

    public void ReInit(List<string> bonus, FixedPoint2 position) {
        _buffer.Init(_bufferValue);
        _rb.center = position;
        _state = PlayerStates.Idle;
        _freezePlayerState = 0;
        _view = null;

        _bonus.Clear();
        _bonus.AddBonus(bonus);
    }

    public void ExecuteInputs(List<string> inputs) {
        if (_state == PlayerStates.Dead) {
            _playerVisual.UpdateVisual(_state, _rb);
            BroadcastInputs(inputs);
            return;
        }

        FixedPoint2 accel = new FixedPoint2(0, 0);

        _buffer.ExecuteInputs(inputs);

        DetectGround();
        DetectWallLeft();
        DetectWallRight();
        Move(ref accel, out FixedPoint dirInput);
        Jump(ref accel);
        bool punch = Punch();
        UseBonus();

        if (_freezePlayerState > 0) {
            _freezePlayerState--;
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

            if (_freezePlayerStateDuration > 0) {
                _freezePlayerState = _freezePlayerStateDuration;
                _freezePlayerStateDuration = 0;
            }
        }

        _rb.acceleration = accel * FP.fp(0.1f);
        _playerVisual.UpdateVisual(_state, _rb);
    }


    private void DetectGround() {
        FixedPoint2 start = new FixedPoint2(_rb.min.x, _rb.center.y);
        FixedPoint2 end = new FixedPoint2(_rb.max.x, _rb.min.y - FP.fp(0.2f));

        List<DPhysxRigidbody> rbs = GlobalManager.Instance.PhysicsManager.BoxCast(start, end, _rb.id);
        if (rbs.Count <= 0)
            _grounded = false;
        else
            _grounded = rbs.Find(x => x.isTrigger == false) != null;

        if (_grounded)
            _coyote = _bufferValue;
        else if (_coyote > 0)
            _coyote--;
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
        FixedPoint airbornRatio = FP.fp(0.7f);

        if (_buffer.GetBufferValue(InputAction.Left) > 0 && _buffer.GetBufferValue(InputAction.Right) > 0)
            directionalInput += _buffer.GetBufferValue(InputAction.Left) > _buffer.GetBufferValue(InputAction.Right) ? FP.fp(-1) : FP.fp(1);
        else if (_buffer.GetBufferedInput(InputAction.Left))
            directionalInput += FP.fp(-1);
        else if (_buffer.GetBufferedInput(InputAction.Right))
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
        if (_coyote > 0)
            if (_buffer.GetBufferedInput(InputAction.Jump)) {
                accel += new FixedPoint2(FixedPoint.zero, _jumpHeight);
                _coyote = 0;
            }
    }

    private bool Punch() {
        if (!_buffer.GetBufferedInput(InputAction.Punch))
            return false;

        DPhysxBox box = new DPhysxBox(_rb.center + new FixedPoint2(FP.fp(1) * _visualDirection, FixedPoint.zero), 1, 1, null, true, true);
        box.tags.Add(AppConst.TAG_HITBOX);
        //Manage possible multi trigger
        box.onTriggerEnter += PlayerHit;
        GlobalManager.Instance.PhysicsManager.AddRigidbody(box, 10);
        _freezePlayerStateDuration = 15;

        return true;
    }

    private void UseBonus() {
        if (!_buffer.GetBufferedInput(InputAction.Bonus))
            return;

        if (_bonus == null)
            return;

        _bonus.UseBonus(0);
    }

    private void PlayerHit(DPhysxRigidbody rb) {
        if (rb.id == _rb.id)
            return;

        if (rb.isStatic || !rb.tags.Contains(AppConst.TAGP_LAYER))
            return;

        FixedPoint projectionForce = FP.fp(0.8f);
        FixedPoint x = FP.Sign(rb.center.x - _rb.center.x);
        FixedPoint y = FP.fp(1);

        rb.velocity += new FixedPoint2(x, y) * projectionForce;

        GameplaySceneManager m = GlobalManager.Instance.SceneManager as GameplaySceneManager;
        m.NotifyHit(_playerIndex, rb);
    }

    public void Die() {
        _state = PlayerStates.Dead;
    }

    public void PlugView(IInputUser view) {
        _view = view;
    }

    private void BroadcastInputs(List<string> inputs) {
        if (_view == null)
            return;

        _view.ExecuteInputs(inputs);
    }
}
