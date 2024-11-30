using System.Collections.Generic;
using UnityEngine;

public class UIViewManager : MonoBehaviour {
    #region Variables
    [SerializeField] private List<UIView> _views = new List<UIView>();

    private int _currentViewIndex;
    #endregion


    public void Init(params object[] parameters) {
        _currentViewIndex = -1;

        for (int i = 0; i < _views.Count; i++)
            HideView(i);

        ShowView(0, parameters);
    }

    public void ShowView(int index, params object[] parameters) {
        if (index == _currentViewIndex || index < 0 || index >= _views.Count) {
            Utils.LogError(this, "ShowView", $"index is {index}");
            return;
        }

        if (_currentViewIndex >= 0 && _currentViewIndex < _views.Count)
            HideView(_currentViewIndex);

        _views[index].gameObject.SetActive(true);
        _views[index].Show(parameters);

        _currentViewIndex = index;
    }

    private void HideView(int index) {
        _views[index].gameObject.SetActive(false);
    }

    public void Back() {
        if (_currentViewIndex > 0)
            ShowView(_currentViewIndex - 1);
    }
}
