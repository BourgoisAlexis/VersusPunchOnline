using UnityEngine;

public abstract class SceneManager : MonoBehaviour {
    [SerializeField] protected UIViewManager _viewManager;

    public virtual void Init(params object[] parameters) {
        if (_viewManager == null) {
            Utils.LogError(this, "Init", "_viewManager is null");
            return;
        }

        _viewManager.Init();
    }

    public virtual void GoToView(int index) {
        _viewManager.ShowView(index);
    }

    public virtual void Back() {
        _viewManager.Back();
    }
}
