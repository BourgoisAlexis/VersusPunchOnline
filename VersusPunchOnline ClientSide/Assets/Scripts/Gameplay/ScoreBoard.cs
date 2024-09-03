using UnityEngine;

public class ScoreBoard : MonoBehaviour {
    [SerializeField] private GameObject _tokenPrefab;
    [SerializeField] private GameObject[] _layouts;

    private int[] _scores;

    public void Init(int playerNumber) {
        _scores = new int[playerNumber];
    }

    public int AddTokenToPlayer(int playerIndex) {
        GameObject go = Instantiate(_tokenPrefab);
        Transform t = go.transform;
        t.SetParent(_layouts[playerIndex].transform);

        _scores[playerIndex]++;

        return _scores[playerIndex];
    }
}
