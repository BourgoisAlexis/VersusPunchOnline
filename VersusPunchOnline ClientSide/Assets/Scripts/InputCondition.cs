using UnityEngine;

public class InputCondition {
    public KeyCode code;
    public bool canMaintain;

    public bool pressed;

    public InputCondition(KeyCode code, bool canMaintain) {
        this.code = code;
        this.canMaintain = canMaintain;
    }
}
