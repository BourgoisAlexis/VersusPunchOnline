using UnityEngine;

public class Cheats {
    public void Update() {
        if (Input.GetKeyDown(KeyCode.F1)) {
            GameplaySceneManager manager = GlobalManager.Instance.SceneManager as GameplaySceneManager;
            if (manager != null) {
                manager.ChooseBonus(0, "Bullet");
            }
        }
    }
}
