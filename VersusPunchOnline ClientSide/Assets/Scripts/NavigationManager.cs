using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class NavigationManager {
    #region Variables
    public Action onLoad;
    public Action onLoaded;

    private int _currentSceneIndex = 0;
    private List<Action> _toClearOnLoaded = new List<Action>();
    #endregion


    public async Task LoadScene(int index) {
        if (index < 0) {
            Utils.LogError(this, "LoadScene", "index is negative");
            return;
        }

        onLoad?.Invoke();

        await GlobalManager.Instance.UITransitionManager.Show();

        AsyncOperation op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(index);
        while (!op.isDone)
            await Task.Yield();

        _currentSceneIndex = index;
        onLoaded?.Invoke();

        foreach (Action action in _toClearOnLoaded)
            onLoaded -= action;

        _toClearOnLoaded.Clear();

        await GlobalManager.Instance.UITransitionManager.Hide();

        Utils.Log(this, "LoadScene", $"loaded scene {_currentSceneIndex}");
    }


    public void AutoClearingActionOnLoad(params Action[] actions) {
        foreach (Action action in actions)
            onLoad += action;

        Action act = () => {
            foreach (Action action in actions)
                onLoad -= action;
        };

        AutoClearingActionOnLoaded(act);
    }

    public void AutoClearingActionOnLoaded(params Action[] actions) {
        foreach (Action action in actions) {
            onLoaded += action;
            _toClearOnLoaded.Add(action);
        }
    }
}
