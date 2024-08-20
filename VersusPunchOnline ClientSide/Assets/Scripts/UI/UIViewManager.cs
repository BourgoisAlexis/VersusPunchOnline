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

    public void ShowView(int index) {
        if (index == _currentViewIndex)
            return;

        if (_currentViewIndex > 0 && _currentViewIndex < _views.Count)
            HideView(_currentViewIndex);

        _views[index].gameObject.SetActive(true);
        _views[index].Init();

        _currentViewIndex = index;
    }

    private void HideView(int index) {
        _views[index].gameObject.SetActive(false);
    }
}
