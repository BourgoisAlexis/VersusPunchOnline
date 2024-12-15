using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class NavigationManager {
    #region Variables
    public Action OnLoad;
    public Action OnLoaded;

    private int _currentSceneIndex = 0;
    private List<Action> _toClearOnLoaded = new List<Action>();
    #endregion


    public async Task LoadScene(int index) {
        if (index < 0) {
            Utils.LogError(this, "LoadScene", "index is negative");
            return;
        }

        OnLoad?.Invoke();

        await GlobalManager.Instance.UITransitionManager.Show();

        AsyncOperation op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(index);
        while (!op.isDone)
            await Task.Yield();

        _currentSceneIndex = index;
        OnLoaded?.Invoke();

        foreach (Action action in _toClearOnLoaded)
            OnLoaded -= action;

        _toClearOnLoaded.Clear();

        await GlobalManager.Instance.UITransitionManager.Hide();

        Utils.Log(this, "LoadScene", $"loaded scene {_currentSceneIndex}");
    }


    public void AutoClearingActionOnLoad(params Action[] actions) {
        foreach (Action action in actions)
            OnLoad += action;

        Action act = () => {
            foreach (Action action in actions)
                OnLoad -= action;
        };

        AutoClearingActionOnLoaded(act);
    }

    public void AutoClearingActionOnLoaded(params Action[] actions) {
        foreach (Action action in actions) {
            OnLoaded += action;
            _toClearOnLoaded.Add(action);
        }
    }
}
