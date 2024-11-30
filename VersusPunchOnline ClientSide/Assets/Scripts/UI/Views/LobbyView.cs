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


    protected override void Init(params object[] parameters) {
        GlobalManager.Instance.PlayerIOManager.HandleMessage(AppConst.serverMessageJoin, DisplayPlayers, 2);
        GlobalManager.Instance.PlayerIOManager.HandleMessage(AppConst.serverMessageRequest, DisplayJoinRequest, 1);

        base.Init(parameters);
    }

    public void DisplayRooms() {

    }

    public void DisplayPlayers(string[] infos) {
        for (int i = 0; i < infos.Length; i += 2) {
            GameObject instantiatedObj = Instantiate(_lobbyPlayerPrefab, _layout);
            instantiatedObj.GetComponent<LobbyPlayer>().Init(infos[i], infos[i + 1]);
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
