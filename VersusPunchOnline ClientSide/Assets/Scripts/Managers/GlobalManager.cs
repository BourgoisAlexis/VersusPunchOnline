using DPhysx;
using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections;

public class GlobalManager : MonoBehaviour {
    #region Variables
    public static GlobalManager Instance;

    [field: SerializeField] public UITransitionManager UITransitionManager { get; private set; }
    [field: SerializeField] public BonusDataBase BonusDataBase { get; private set; }
    public NavigationManager NavigationManager { get; private set; }
    public PlayerIOManager PlayerIOManager { get; private set; }
    public InputManager InputManager { get; private set; }
    public DPhysxManager PhysicsManager { get; private set; }
    public GameStateManager GameStateManager { get; private set; }
    public SceneManager SceneManager { get; private set; }

    public PeerConnection<InputMessage> Connection { get; private set; }
    public int SelfID { get; set; }
    public bool IsLocal { get; set; }

    //FixedUpdate
    public Action OnCustomUpdate;
    //Update
    public Action OnSecondaryCustomUpdate;

    [Header("Debug params")]
    public bool UseLocalPlayerIO = true;
    public bool ShowLowPriorityLogs = true;
    public bool SlowMode = true;

    private float _customUpdateTimer = 0;
    private float _secondaryCustomUpdateTimer = 0;
    #endregion


    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
        else {
            Utils.LogError("Duplicated Singleton");
            Destroy(gameObject);
        }

        NavigationManager = new NavigationManager();
        PlayerIOManager = new PlayerIOManager();
        Connection = new SimpleUDPConnection<InputMessage>();
        GameStateManager = new GameStateManager();

        PhysicsManager = GetComponent<DPhysxManager>();
        InputManager = GetComponent<InputManager>();

        DontDestroyOnLoad(gameObject);

        GetSceneManager();
        NavigationManager.OnLoaded += GetSceneManager;

        NavigationManager.LoadScene(1);
    }

    private void Update() {
        PlayerIOManager.ProcessMessages();
    }

    private void FixedUpdate() {
        _customUpdateTimer += Time.fixedDeltaTime;
        _secondaryCustomUpdateTimer += Time.fixedDeltaTime;

        if (_customUpdateTimer >= (SlowMode ? AppConst.SECONDARY_CUSTOM_UPDATE_RATE : AppConst.CUSTOM_UPDATE_RATE)) {
            OnCustomUpdate?.Invoke();
            _customUpdateTimer = 0;
        }

        if (_secondaryCustomUpdateTimer >= AppConst.SECONDARY_CUSTOM_UPDATE_RATE) {
            OnSecondaryCustomUpdate?.Invoke();
            _secondaryCustomUpdateTimer = 0;
        }
    }

    private void OnApplicationQuit() {
        if (IsLocal)
            return;

        Connection.CloseConnection();
    }

    private void GetSceneManager() {
        SceneManager = FindObjectOfType<SceneManager>();
        SceneManager?.Init();
    }

    public async Task TaskWithDelay(float duration) {
        bool goOn = false;
        StartCoroutine(WaitCoroutine(duration, () => { goOn = true; }));
        while (!goOn)
            await Task.Yield();
    }

    public IEnumerator WaitCoroutine(float duration, Action onEnd) {
        yield return new WaitForSecondsRealtime(duration);
        onEnd?.Invoke();
    }
}