
using UnityEngine;

public enum InputAction {
    Neutral,
    Left,
    Right,
    Up,
    Down,

    Punch,
    Jump,

    Valid,
    Cancel,

    Default
}

public class InputCondition {
    private string _id;
    public InputAction action;
    private KeyCode _key;
    //Is still valid when not released
    private bool _canMaintain;

    private bool _pressed;
    private int _axisValue;


    public InputCondition(string id, InputAction action, bool canMaintain, int axisValue = 0) {
        _id = id;
        this.action = action;
        _canMaintain = canMaintain;
        _axisValue = axisValue;
    }

    public InputCondition(KeyCode key, InputAction action, bool canMaintain) {
        _key = key;
        this.action = action;
        _canMaintain = canMaintain;
    }

    public bool IsValid() {
        if (string.IsNullOrEmpty(_id))
            return IsValidKey();

        bool valid = false;

        if (_axisValue != 0) {
            float value = Input.GetAxisRaw(_id);

            if (value != 0 && Mathf.Sign(value) == _axisValue)
                return true;
        }

        if (Input.GetButton(_id)) {
            if (_canMaintain)
                valid = true;
            else
                valid = !_pressed;

            _pressed = true;
        }
        else if (!Input.GetButton(_id)) {
            _pressed = false;
        }

        return valid;
    }

    private bool IsValidKey() {
        bool valid = false;

        if (Input.GetKey(_key)) {
            if (_canMaintain)
                valid = true;
            else
                valid = !_pressed;

            _pressed = true;
        }
        else if (!Input.GetKey(_key)) {
            _pressed = false;
        }

        return valid;
    }
}
