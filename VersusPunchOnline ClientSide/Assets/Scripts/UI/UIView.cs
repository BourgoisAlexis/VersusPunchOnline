using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UIButtonRow {
    public List<UIButton> buttons;
}


public class UIView : MonoBehaviour, IInputUser {
    #region Variables
    [SerializeField] protected UIButton _backButton;
    [SerializeField] protected List<UIButtonRow> _buttons = new List<UIButtonRow>();

    private Vector2Int _currentButton;
    private int _currentDelay;
    private int _inputLimit = 10;
    private bool _initialized;

    public UIButton GetCurrentButton() => _buttons[_currentButton.y].buttons[_currentButton.x];
    #endregion


    protected virtual void Init(params object[] parameters) {
        if (_buttons == null || _buttons.Count <= 0)
            return;

        GetCurrentButton().OnPointerEnter(null);

        GlobalManager.Instance.InputManager.AddListener(this);

        _backButton?.OnClick.AddListener(Back);
        _initialized = true;
    }

    private void OnDestroy() {
        GlobalManager.Instance.InputManager.RemoveListener(this);
    }

    public virtual void Show(params object[] parameters) {
        if (!_initialized)
            Init(parameters);
    }

    public virtual void Back() {
        GlobalManager.Instance.SceneManager.Back();
    }

    public void ExecuteInputs(List<string> inputs) {
        if (_buttons == null || _buttons.Count <= 0)
            return;

        if (inputs == null)
            return;

        if (_currentDelay > 0) {
            _currentDelay--;
            return;
        }
        else if (inputs.Count > 1)
            _currentDelay = _inputLimit;

        if (inputs.Contains(InputAction.Valid.ToString())) {
            GetCurrentButton().OnPointerClick(null);
            return;
        }

        Vector2Int dir = new Vector2Int();

        if (inputs.Contains(InputAction.Left.ToString()))
            dir.x = -1;
        else if (inputs.Contains(InputAction.Right.ToString()))
            dir.x = 1;

        Navigate(dir);
    }

    public void Navigate(Vector2Int direction) {
        if (direction == Vector2Int.zero)
            return;

        Vector2Int result = _currentButton + direction;

        if (result.y < 0 || result.y >= _buttons.Count)
            return;

        if (result.x < 0 || result.x >= _buttons[result.y].buttons.Count)
            return;

        GetCurrentButton().OnPointerExit(null);
        _currentButton = result;
        GetCurrentButton().OnPointerEnter(null);
    }
}
