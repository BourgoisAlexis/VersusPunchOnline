using TMPro;
using UnityEngine;

public class LobbyView : UIView {
    #region Variables
    [SerializeField] private Transform _layout;
    [SerializeField] private GameObject _lobbyPlayerPrefab;

    [SerializeField] private TextMeshProUGUI _tmproMessage;
    [SerializeField] private UIButton _buttonAccept;
    [SerializeField] private UIButton _buttonDecline;
    #endregion


    public override void Init(params object[] parameters) {
        GlobalManager.Instance.PlayerIOManager.AddMessageToHandle(AppConst.serverMessageJoin, DisplayPlayers);
        GlobalManager.Instance.PlayerIOManager.AddMessageToHandle(AppConst.serverMessageRequest, DisplayJoinRequest);
    }

    public void DisplayRooms() {

    }

    public void DisplayPlayers(string[] infos) {
        foreach (string player in infos) {
            GameObject instantiatedObj = Instantiate(_lobbyPlayerPrefab, _layout);
            string[] subStrings = player.Split(',');
            instantiatedObj.GetComponent<LobbyPlayer>().Init(subStrings[0], subStrings[1]);
        }
    }

    public void DisplayJoinRequest(string[] infos) {
        string[] subStrings = infos[0].Split(',');
        _tmproMessage.text = $"{subStrings[1]} asks you for a game";
        _buttonAccept.onClick.AddListener(() => {
            GlobalManager.Instance.PlayerIOManager.SendMessage(AppConst.userMessageAcceptRequest, subStrings[0]);
        });

        _buttonAccept.onClick.AddListener(() => {
            GlobalManager.Instance.PlayerIOManager.SendMessage(AppConst.userMessageDeclineRequest, subStrings[0]);
        });
    }
}
