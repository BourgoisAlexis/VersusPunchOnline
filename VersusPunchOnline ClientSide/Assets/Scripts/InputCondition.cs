
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
    public string id;
    public InputAction input;
    //Is still valid when not released
    public bool canMaintain;

    private bool pressed;
    private int axisValue;

    public InputCondition(string id, InputAction input, bool canMaintain, int axisValue = 0) {
        this.id = id;
        this.input = input;
        this.canMaintain = canMaintain;
        this.axisValue = axisValue;
    }

    public bool IsValid() {
        bool valid = false;

        if (axisValue != 0) {
            float value = Input.GetAxisRaw(id);

            if (value != 0 && Mathf.Sign(value) == axisValue)
                return true;
        }

        if (Input.GetButtonDown(id)) {
            if (canMaintain)
                valid = true;
            else
                valid = !pressed;

            pressed = true;
        }
        else if (!Input.GetButton(id)) {
            pressed = false;
        }

        return valid;
    }
}
