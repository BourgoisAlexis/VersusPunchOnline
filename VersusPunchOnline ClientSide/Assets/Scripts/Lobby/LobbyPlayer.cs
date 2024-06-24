using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayer : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _tmproUserName;
    [SerializeField] private Button _button;

    public void Setup(string userID, string userName) {
        _tmproUserName.text = userName;
        _button.onClick.AddListener(() => {
            GlobalManager.Instance.playerIOManager.SendMessage(AppConst.userMessageRequestPTP, userID);
        });
    }
}
