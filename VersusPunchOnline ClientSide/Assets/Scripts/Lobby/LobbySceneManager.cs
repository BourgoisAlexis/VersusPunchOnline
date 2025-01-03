using System.Net;
using System.Threading.Tasks;


public class LobbySceneManager : SceneManager {
    public override void Init(params object[] parameters) {
        base.Init(parameters);
        GlobalManager.Instance.InputManager.InitMainScreen();
        GlobalManager.Instance.GameStateManager.ChangeGameState(GameState.MainScreen);
    }

    public void SetupLocal() {
        LoadGameplayScene(true);
    }

    public void SetupOnline() {
        ConnectToPlayerIO();
    }

    public async void SetupPhysics() {
        await GlobalManager.Instance.NavigationManager.LoadScene(3);
        GlobalManager.Instance.PhysicsManager.Init();
    }

    public async void SetupNetwork() {
        await GlobalManager.Instance.NavigationManager.LoadScene(4);
    }

    private void ConnectToPlayerIO() {
        GlobalManager.Instance.PlayerIOManager.HandleMessage(AppConst.serverMessageAskForConnectionInfos, OpenConnection, 2);
        GlobalManager.Instance.PlayerIOManager.HandleMessage(AppConst.serverMessageConnectionInfos, ConnectToHost, 2);
        GlobalManager.Instance.PlayerIOManager.Init("versuspunchonline-hxzulsresk6ho8sj0rffgq", $"Alexis_{UnityEngine.Random.Range(0, 100)}", () => {
            GlobalManager.Instance.PlayerIOManager.CreateRoom(null, null);
        });

        _viewManager.ShowView(1);
    }

    private void OpenConnection(string[] infos) {
        string receiverID = infos[0];
        GlobalManager.Instance.SelfID = int.Parse(infos[1]);

        GlobalManager.Instance.Connection.Init((iPEndPoint) => {
            string ip = iPEndPoint.Address.ToString();
            string port = iPEndPoint.Port.ToString();
            GlobalManager.Instance.PlayerIOManager.SendMessage(AppConst.userMessageP2POpen, receiverID, ip, port);
        }, true, 1);
    }

    private async void ConnectToHost(string[] infos) {
        IPAddress ip = IPAddress.Parse(infos[0]);
        int port = int.Parse(infos[1]);
        IPEndPoint endPoint = new IPEndPoint(ip, port);

        bool goOn = false;
        GlobalManager.Instance.Connection.Connect(endPoint, () => {
            goOn = true;
        });

        while (!goOn)
            await Task.Yield();

        LoadGameplayScene(false);
    }

    private async void LoadGameplayScene(bool isLocal) {
        GlobalManager.Instance.IsLocal = isLocal;
        await GlobalManager.Instance.NavigationManager.LoadScene(2);
    }
}
