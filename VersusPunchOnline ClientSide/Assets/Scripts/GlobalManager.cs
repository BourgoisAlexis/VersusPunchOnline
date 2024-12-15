using DPhysx;
using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.SceneManagement;

public class GlobalManager : MonoBehaviour {
    #region Variables
    public static GlobalManager Instance;

    [field : SerializeField] public UITransitionManager UITransitionManager {get; private set;}
    [field: SerializeField] public BonusDataBase BonusDataBase { get; private set; }
    public NavigationManager NavigationManager { get; private set; }
    public PlayerIOManager PlayerIOManager { get; private set; }
    public SimpleUDPConnection<InputMessage> ConnectionManager { get; private set; }
    public InputManager InputManager { get; private set; }
    public DPhysxManager PhysicsManager { get; private set; }
    public GameStateManager GameStateManager { get; private set; }
    public SceneManager SceneManager { get; private set; }
    public int SelfID { get; set; }
    public bool IsLocal { get; set; }

    //FixedUpdate
    public Action onCustomUpdate;
    //Update
    public Action onSecondaryCustomUpdate;


    [Header("Debug params")]
    public bool UseLocalPlayerIO = true;
    public bool ShowLowPriorityLogs = true;
    [Tooltip("Base FixedUpdate rate on the secondary update rate")]
    public bool SlowMode = true;

    private float customUpdateTimer = 0;
    private float secondaryCustomUpdateTimer = 0;
    #endregion


    private void Awake() {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        NavigationManager = new NavigationManager();
        PlayerIOManager = new PlayerIOManager();
        ConnectionManager = new SimpleUDPConnection<InputMessage>();
        GameStateManager = new GameStateManager();

        PhysicsManager = GetComponent<DPhysxManager>();
        InputManager = GetComponent<InputManager>();

        DontDestroyOnLoad(gameObject);

        GetSceneManager();
        NavigationManager.onLoaded += GetSceneManager;

        NavigationManager.LoadScene(1);
    }

    private void Update() {
        PlayerIOManager.ProcessMessages();
    }

    private void FixedUpdate() {
        customUpdateTimer += Time.fixedDeltaTime;
        secondaryCustomUpdateTimer += Time.fixedDeltaTime;

        if (customUpdateTimer >= (SlowMode ? AppConst.secondaryCustomUpdateRate : AppConst.customUpdateRate)) {
            onCustomUpdate?.Invoke();
            customUpdateTimer = 0;
        }

        if (secondaryCustomUpdateTimer >= AppConst.secondaryCustomUpdateRate) {
            onSecondaryCustomUpdate?.Invoke();
            secondaryCustomUpdateTimer = 0;
        }
    }

    private void OnApplicationQuit() {
        if (IsLocal)
            return;

        ConnectionManager.CloseConnection();
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