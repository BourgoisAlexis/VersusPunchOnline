using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayer : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _tmproUserName;
    [SerializeField] private UIButton _button;

    public void Init(string userID, string userName) {
        _tmproUserName.text = userName;
        _button.onClick.AddListener(() => {
            GlobalManager.Instance.PlayerIOManager.SendMessage(AppConst.userMessageRequestPTP, userID);
        });
    }
}
