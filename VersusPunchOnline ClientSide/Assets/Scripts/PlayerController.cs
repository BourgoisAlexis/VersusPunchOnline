using DPhysx;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    #region Variables
    [SerializeField] private Transform _visual;

    private DPhysxShape _shape;
    private FixedPoint _visualDirection;
    #endregion


    public void Setup() {
        _shape = GetComponent<DPhysxShapeMonobehaviour>().shape;
        _shape.tags.Add(AppConst.tagPlayer);
    }

    public void ExecuteInputs(FrameData frame) {
        string json = JsonUtility.ToJson(frame);
        FixedPoint2 directionalInput = new FixedPoint2(0, 0);
        FixedPoint2 accel = new FixedPoint2(0, 0);

        foreach (string s in frame.inputs.Split(';')) {
            KeyCode input = (KeyCode)System.Enum.Parse(typeof(KeyCode), s);

            if (input == KeyCode.LeftArrow)
                directionalInput.x += FP.fp(-1);
            else if (input == KeyCode.RightArrow)
                directionalInput.x += FP.fp(1);

            if (input == KeyCode.UpArrow)
                directionalInput.y += FP.fp(1);

            if (input == KeyCode.Space)
                Hit();
        }

        Move(directionalInput, ref accel);

        Jump(directionalInput, ref accel);

        _shape.acceleration = accel * FP.fp(0.1f);
    }


    private void Move(FixedPoint2 input, ref FixedPoint2 accel) {
        if (input.x == FixedPoint.zero)
            accel -= new FixedPoint2(_shape.velocity.normalized.x * FP.fp(0.5f), FixedPoint.zero);
        else if (FP.Abs(_shape.velocity.x) < FP.fp(0.5f))
            accel += new FixedPoint2(input.x, FP.fp(0));

        if (input.x != FixedPoint.zero) {
            if (FP.Sign(_shape.velocity.x) == FP.Sign(input.x))
                return;

            bool goingLeft = input.x < FixedPoint.zero;
            _visual.localEulerAngles = new Vector2(0, 180 * (goingLeft ? 1 : 0));
            _visualDirection = goingLeft ? FP.fp(-1) : FP.fp(1);
        }
    }

    private void Jump(FixedPoint2 input, ref FixedPoint2 accel) {
        if (input.y > FixedPoint.zero && _shape.velocity.y == FixedPoint.zero)
            accel += new FixedPoint2(0, 10);
    }

    private void Hit() {
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
