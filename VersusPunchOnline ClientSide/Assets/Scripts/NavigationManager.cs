using System;
using System.Threading.Tasks;
using UnityEngine;

public class NavigationManager {
    public Action onLoadScene;
    public Action onSceneLoaded;

    private int _currentSceneIndex = 0;

    public async Task LoadScene(int index) {
        onLoadScene?.Invoke();

        await GlobalManager.Instance.UITransitionManager.Show();

        AsyncOperation op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(index);
        while (!op.isDone)
            await Task.Yield();

        _currentSceneIndex = index;
        onSceneLoaded?.Invoke();

        await GlobalManager.Instance.UITransitionManager.Hide();

        Utils.Log(this, "LoadScene", $"loaded scene {_currentSceneIndex}");
    }
}
