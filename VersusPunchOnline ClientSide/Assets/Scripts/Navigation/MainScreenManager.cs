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
        await GlobalManager.Instance.NavigationManager.LoadScene(2);
        GlobalManager.Instance.PhysicsManager.Setup();
    }

    public async void SetupNetwork() {
        await GlobalManager.Instance.NavigationManager.LoadScene(3);
    }

    public void ConnectToPlayerIO() {
        GlobalManager.Instance.PlayerIOManager.AddMessageToHandle(AppConst.serverMessageAskForConnectionInfos, OpenConnection);
        GlobalManager.Instance.PlayerIOManager.AddMessageToHandle(AppConst.serverMessageConnectionInfos, ConnectToHost);
        GlobalManager.Instance.PlayerIOManager.Init("Alexis", null);

        _viewManager.ShowView(1);
    }

    public void OpenConnection(string[] infos) {
        string receiverID = infos[0];
        GlobalManager.Instance.selfID = int.Parse(infos[1]);

        GlobalManager.Instance.ConnectionManager.Init((iPEndPoint) => {
            string ip = iPEndPoint.Address.ToString();
            string port = iPEndPoint.Port.ToString();
            GlobalManager.Instance.PlayerIOManager.SendMessage(AppConst.userMessageP2POpen, receiverID, ip, port);
        });
    }

    public async void ConnectToHost(string[] infos) {
        IPAddress ip = IPAddress.Parse(infos[0]);
        int port = int.Parse(infos[1]);
        IPEndPoint endPoint = new IPEndPoint(ip, port);

        bool goOn = false;
        GlobalManager.Instance.ConnectionManager.Connect(endPoint, () => {
            goOn = true;
        });

        while (!goOn)
            await Task.Yield();

        LoadGameplayScene(false);
    }

    private async void LoadGameplayScene(bool isLocal) {
        GlobalManager.Instance.isLocal = isLocal;
        await GlobalManager.Instance.NavigationManager.LoadScene(1);
        await Task.Delay(500);
        GlobalManager.Instance.PhysicsManager.Setup();
        GlobalManager.Instance.InputManager.Init();
        GlobalManager.Instance.PlayerIOManager.LeaveRoom();
    }
}
