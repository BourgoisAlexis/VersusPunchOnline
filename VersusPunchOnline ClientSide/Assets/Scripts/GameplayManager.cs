using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;

public class GameplayManager : SceneManager {
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private List<Transform> _spawnPoints;

    public override async void Init(params object[] parameters) {
        List<PlayerController> ctrls = new List<PlayerController>();
        foreach (Transform point in _spawnPoints) {
            GameObject go = Instantiate(_playerPrefab);
            Transform t = go.transform;
            t.SetParent(point);
            t.localPosition = Vector3.zero;
            ctrls.Add(go.GetComponent<PlayerController>());
            await Task.Delay(500);
        }

        GlobalManager.Instance.PhysicsManager.Setup();
        GlobalManager.Instance.InputManager.GameplayInit(ctrls);
        GlobalManager.Instance.PlayerIOManager.LeaveRoom();
    }
}
