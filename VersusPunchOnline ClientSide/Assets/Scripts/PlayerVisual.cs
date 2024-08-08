using DPhysx;
using UnityEngine;

public class PlayerVisual : MonoBehaviour {
    [SerializeField] private Animation _animation;
    [SerializeField] private Anim[] _anims;

    [SerializeField][Range(1, 10)] private int _sampleRate = 2;

    private Transform _t;
    private int _animationFrameRate = 60;
    private int _spacing = 0;

    private string _currentAnim;
    private int _currentFrame;
    private int _length;

    [Header("Test")]
    public Anim testAnim;
    public int testFrame;


    private void Awake() {
        _t = transform;
        foreach (var anim in _anims)
            _animation.AddClip(anim.clip, anim.name);

        ChangeAnim("Idle");
    }

    private void Start() {
        GlobalManager.Instance.onCustomUpdate += PlayAnim;
    }

    private void Update() {
        //if (Input.GetKeyDown(KeyCode.LeftArrow))
        //    ChangeAnim("Run");
        //else if (Input.GetKeyDown(KeyCode.UpArrow))
        //    ChangeAnim("Jump");
        //else if (Input.GetKeyDown(KeyCode.Space))
        //    ChangeAnim("Punch");
        //else if (Input.GetKeyDown(KeyCode.KeypadEnter))
        //    ChangeAnim("Idle");
    }


    public void UpdateVisual(PlayerStates state, DPhysxRigidbody rb) {
        switch (state) {
            case PlayerStates.Idle:
                ChangeAnim("Idle");
                break;

            case PlayerStates.Run:
                ChangeAnim("Run");
                _t.localEulerAngles = new Vector2(0, 180 * (rb.velocity.x < FixedPoint.zero ? 1 : 0));
                break;

            case PlayerStates.Midair:
                ChangeAnim("Jump");
                break;

            case PlayerStates.Punch:
                ChangeAnim("Punch");
                break;
        }
    }


    private void JumpToFrame(float frame) {
        float time = frame / 60;
        _animation[_currentAnim].time = time;
        _animation.Play(_currentAnim);
        _animation[_currentAnim].speed = 0;
    }

    private void ChangeAnim(string anim) {
        if (anim == _currentAnim)
            return;

        _currentAnim = anim;
        _currentFrame = 0;
        _length = (int)(_animation[_currentAnim].length * _animationFrameRate);
    }

    private void PlayAnim() {
        _spacing++;

        if (_spacing < _sampleRate)
            return;

        _spacing = 0;
        _currentFrame += _sampleRate;

        if (_currentFrame > _length)
            _currentFrame = 0;

        JumpToFrame(_currentFrame);
    }


    private void OnDrawGizmos() {
        if (Application.isPlaying)
            return;

        if (testAnim == null)
            return;

        int length = (int)(testAnim.clip.length * _animationFrameRate);

        if (testFrame > length)
            return;

        AnimHitBox hitBox = testAnim.hitBoxes.Find(x => x.frame == testFrame);

        if (hitBox == null)
            return;

        Gizmos.color = AppConst.HitBoxColor;
        Gizmos.DrawWireCube((Vector2)transform.position + hitBox.position, hitBox.size);
    }
}
