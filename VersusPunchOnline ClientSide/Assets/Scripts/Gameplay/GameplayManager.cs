using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;
using DPhysx;

public class GameplayManager : SceneManager {
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private List<Transform> _spawnPoints;
    [SerializeField] private ScoreBoard _scoreBoard;

    private List<PlayerController> _playerControllers = new List<PlayerController>();
    private List<List<string>> _bonus = new List<List<string>>();


    public override async void Init(params object[] parameters) {
        base.Init(parameters);

        foreach (Transform point in _spawnPoints) {
            GameObject go = Instantiate(_playerPrefab);
            Transform t = go.transform;
            t.SetParent(point);
            t.SetParent(point);
            _playerControllers.Add(go.GetComponent<PlayerController>());
        }

        await Task.Delay(500);

        for (int i = 0; i < _playerControllers.Count; i++) {
            FixedPoint2 position = FixedPoint2.FromVector2(_spawnPoints[i].transform.position);
            _playerControllers[i].Init(i, position);
            _bonus.Add(new List<string>());
        }

        GlobalManager.Instance.BonusDataBase.Init();
        GlobalManager.Instance.PhysicsManager.Init();
        GlobalManager.Instance.PlayerIOManager.LeaveRoom();
        GlobalManager.Instance.InputManager.InitGameplay(_playerControllers);
        GlobalManager.Instance.GameStateManager.ChangeGameState(GameState.Gameplay);

        _scoreBoard.Init(2);
    }

    public void NotifyHit(int hitterIndex, DPhysxRigidbody rb) {
        PlayerController ctrl = rb.t.gameObject.GetComponent<PlayerController>();
        int hittedIndex = _playerControllers.IndexOf(ctrl);

        ctrl.Die();

        int score = _scoreBoard.AddTokenToPlayer(hitterIndex);

        if (score >= 4) {
            GameplayManager m = GlobalManager.Instance.UniqueSceneManager as GameplayManager;
            m.ReturnToMainScreen();
            return;
        }

        Bonus b1 = GlobalManager.Instance.BonusDataBase.GetRandomBonus();
        Bonus b2 = GlobalManager.Instance.BonusDataBase.GetRandomBonus();

        _viewManager.ShowView(1, b1, b2, hittedIndex, ctrl);
    }

    public void ChooseBonus(int playerIndex, string bonusId) {
        _bonus[playerIndex].Add(bonusId);
        Utils.Log(this, "ChooseBonus", $"Player {playerIndex} choose bonus {bonusId}");
        _viewManager.ShowView(0);
        NewRound();
    }

    private void NewRound() {
        for (int i = 0; i < _playerControllers.Count; i++) {
            FixedPoint2 position = FixedPoint2.FromVector2(_spawnPoints[i].transform.position);
            _playerControllers[i].ReInit(_bonus[i], position);
        }
    }

    public async void ReturnToMainScreen() {
        GlobalManager.Instance.NavigationManager.LoadScene(1);
    }
}
