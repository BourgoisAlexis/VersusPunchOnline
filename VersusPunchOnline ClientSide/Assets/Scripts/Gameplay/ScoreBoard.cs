using UnityEngine;

public class ScoreBoard : MonoBehaviour {
    [SerializeField] private GameObject _tokenPrefab;
    [SerializeField] private GameObject[] _layouts;

    private int[] _scores;

    public void Init(int playerNumber) {
        _scores = new int[playerNumber];
    }

    public void AddTokenToPlayer(int playerIndex) {
        GameObject go = Instantiate(_tokenPrefab);
        Transform t = go.transform;
        t.SetParent(_layouts[playerIndex].transform);

        _scores[playerIndex]++;

        if (_scores[playerIndex] >= 4) {
            GameplayManager m = GlobalManager.Instance.SceneManager as GameplayManager;
            m.ReturnToMainScreen();
        }
    }
}
