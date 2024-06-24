using System.Collections.Generic;
using UnityEngine;

public class UIViewManager : MonoBehaviour {
    [SerializeField] private List<UIView> _views = new List<UIView>();
    private int _currentViewIndex;

    public void ShowView(int index) {
        if (index == _currentViewIndex)
            return;

        _views[_currentViewIndex].gameObject.SetActive(false);
        _views[index].gameObject.SetActive(true);
        _views[index].Setup();

        _currentViewIndex = index;
    }
}
