using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyView : UIView {
    [SerializeField] private Transform _layout;
    [SerializeField] private GameObject _lobbyPlayerPrefab;

    [SerializeField] private TextMeshProUGUI _tmproMessage;
    [SerializeField] private Button _buttonAccept;
    [SerializeField] private Button _buttonDecline;

    public override void Setup(params object[] parameters) {
        GlobalManager.Instance.playerIOManager.AddMessageToHandle(AppConst.serverMessageJoin, DisplayPlayers);
        GlobalManager.Instance.playerIOManager.AddMessageToHandle(AppConst.serverMessageRequest, DisplayJoinRequest);
    }

    public void DisplayRooms() {

    }

    public void DisplayPlayers(string[] infos) {
        foreach (string player in infos) {
            GameObject instantiatedObj = Instantiate(_lobbyPlayerPrefab, _layout);
            string[] subStrings = player.Split(',');
            instantiatedObj.GetComponent<LobbyPlayer>().Setup(subStrings[0], subStrings[1]);
        }
    }

    public void DisplayJoinRequest(string[] infos) {
        string[] subStrings = infos[0].Split(',');
        _tmproMessage.text = $"{subStrings[1]} asks you for a game";
        _buttonAccept.onClick.AddListener(() => {
            GlobalManager.Instance.playerIOManager.SendMessage(AppConst.userMessageAcceptRequest, subStrings[0]);
        });

        _buttonAccept.onClick.AddListener(() => {
            GlobalManager.Instance.playerIOManager.SendMessage(AppConst.userMessageDeclineRequest, subStrings[0]);
        });
    }
}
