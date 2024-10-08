using System.Collections.Generic;
using UnityEngine;

public class UIViewManager : MonoBehaviour {
    [SerializeField] private List<UIView> _views = new List<UIView>();
    private int _currentViewIndex = -1;

    public void Init() {
        for (int i = 0; i < _views.Count; i++)
            HideView(i);

        ShowView(0);
    }

    public void ShowView(int viewIndex, params object[] parameters) {
        if (viewIndex == _currentViewIndex)
            return;

        if (_currentViewIndex >= 0 && _currentViewIndex < _views.Count)
            HideView(_currentViewIndex);

        _views[viewIndex].gameObject.SetActive(true);
        _views[viewIndex].Init(parameters);

        _currentViewIndex = viewIndex;
    }

    private void HideView(int index) {
        GlobalManager.Instance.InputManager.RemoveListener(_views[index]);
        _views[index].gameObject.SetActive(false);
    }
}
