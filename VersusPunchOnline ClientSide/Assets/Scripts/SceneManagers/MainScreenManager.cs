using System.Net;
using System.Threading.Tasks;

public class MainScreenManager : SceneManager {
    private UIViewManager _viewManager;

    private void Awake() {
        _viewManager = GetComponent<UIViewManager>();
    }

    public void SetupLocal() {
        LoadGameplayScene(true);
    }

    public async void SetupPhysics() {
        await GlobalManager.Instance.navigationManager.LoadScene(2);
        GlobalManager.Instance.dPhysxManager.Setup();
    }

    public async void SetupNetwork() {
        await GlobalManager.Instance.navigationManager.LoadScene(3);
    }

    public void ConnectToPlayerIO() {
        GlobalManager.Instance.playerIOManager.AddMessageToHandle(AppConst.serverMessageAskForConnectionInfos, OpenPTPConnection);
        GlobalManager.Instance.playerIOManager.AddMessageToHandle(AppConst.serverMessageConnectionInfos, ConnectToPTP);
        GlobalManager.Instance.playerIOManager.Setup("Alexis", null);

        _viewManager.ShowView(1);
    }

    public void OpenPTPConnection(string[] infos) {
        string receiverID = infos[0];
        GlobalManager.Instance.selfID = int.Parse(infos[1]);

        GlobalManager.Instance.peerToPeerManager.Setup((iPEndPoint) => {
            string ip = iPEndPoint.Address.ToString();
            string port = iPEndPoint.Port.ToString();
            GlobalManager.Instance.playerIOManager.SendMessage(AppConst.userMessagePTPOpen, receiverID, ip, port);
        });
    }

    public async void ConnectToPTP(string[] infos) {
        IPAddress ip = IPAddress.Parse(infos[0]);
        int port = int.Parse(infos[1]);
        IPEndPoint endPoint = new IPEndPoint(ip, port);
        bool goOn = false;
        GlobalManager.Instance.peerToPeerManager.Connect(endPoint, () => {
            goOn = true;
        });

        while (!goOn)
            await Task.Yield();

        LoadGameplayScene(false);
    }

    private async void LoadGameplayScene(bool isLocal) {
        GlobalManager.Instance.isLocal = isLocal;
        await GlobalManager.Instance.navigationManager.LoadScene(1);
        await Task.Delay(500);
        GlobalManager.Instance.dPhysxManager.Setup();
        GlobalManager.Instance.inputManager.Setup();
        GlobalManager.Instance.playerIOManager.LeaveRoom();
    }
}
