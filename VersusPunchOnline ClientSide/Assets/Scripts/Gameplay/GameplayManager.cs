using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using DPhysx;

public class GameplayManager : SceneManager {
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private List<Transform> _spawnPoints;

    private List<PlayerController> _playerControllers = new List<PlayerController>();


    public override async void Init(params object[] parameters) {
        foreach (Transform point in _spawnPoints) {
            GameObject go = Instantiate(_playerPrefab);
            Transform t = go.transform;
            t.SetParent(point);
            t.localPosition = Vector3.zero;
            _playerControllers.Add(go.GetComponent<PlayerController>());
            await Task.Delay(500);
        }

        for (int i = 0; i < _playerControllers.Count; i++)
            _playerControllers[i].Init(i);

        GlobalManager.Instance.PhysicsManager.Setup();
        GlobalManager.Instance.InputManager.GameplayInit(_playerControllers);
        GlobalManager.Instance.PlayerIOManager.LeaveRoom();
    }

    public void NotifyHit(DPhysxRigidbody rb) {
        PlayerController ctrl = rb.t.gameObject.GetComponent<PlayerController>();
        ctrl.Die();
    }
}
