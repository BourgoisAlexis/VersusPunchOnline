using DPhysx;
using System;
using UnityEngine;

public class GlobalManager : MonoBehaviour {
    #region Variables
    public static GlobalManager Instance;

    [SerializeField] private UITransitionManager _uiTransitionManager;
    [SerializeField] private BonusDataBase _bonusDataBase;
    private NavigationManager _navigationManager;
    private PlayerIOManager _playerIOManager;
    private UDPGameplay _connectionManager;
    private InputManager _inputManager;
    private DPhysxManager _dPhysxManager;
    private SceneManager _sceneManager;
    private GameStateManager _gameStateManager;

    [Header("Debug params")]
    public bool useLocalPlayerIO = true;
    public bool showLowPriorityLogs = true;
    [Tooltip("Base FixedUpdate rate on the secondary update rate")]
    public bool slowMode = true;
    public bool isLocal { get; set; }

    private float customUpdateTimer = 0;
    private float secondaryCustomUpdateTimer = 0;
    //FixedUpdate
    public Action onCustomUpdate;
    //Update
    public Action onSecondaryCustomUpdate;

    public int selfID { get; set; }

    //Accessors
    public UITransitionManager UITransitionManager => _uiTransitionManager;
    public BonusDataBase BonusDataBase => _bonusDataBase;
    public NavigationManager NavigationManager => _navigationManager;
    public PlayerIOManager PlayerIOManager => _playerIOManager;
    public UDPGameplay ConnectionManager => _connectionManager;
    public InputManager InputManager => _inputManager;
    public DPhysxManager PhysicsManager => _dPhysxManager;
    public GameStateManager GameStateManager => _gameStateManager;
    public SceneManager SceneManager => _sceneManager;
    #endregion


    private void Awake() {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        _navigationManager = new NavigationManager();
        _playerIOManager = new PlayerIOManager();
        _connectionManager = new UDPGameplay();
        _gameStateManager = new GameStateManager();

        _dPhysxManager = GetComponent<DPhysxManager>();
        _inputManager = GetComponent<InputManager>();

        DontDestroyOnLoad(gameObject);

        GetSceneManager();
        _navigationManager.onLoaded += GetSceneManager;

        _navigationManager.LoadScene(1);
    }

    private void Update() {
        _playerIOManager.ProcessMessages();
    }

    private void FixedUpdate() {
        customUpdateTimer += Time.fixedDeltaTime;
        secondaryCustomUpdateTimer += Time.fixedDeltaTime;

        if (customUpdateTimer >= (slowMode ? AppConst.secondaryCustomUpdateRate : AppConst.customUpdateRate)) {
            onCustomUpdate?.Invoke();
            customUpdateTimer = 0;
        }

        if (secondaryCustomUpdateTimer >= AppConst.secondaryCustomUpdateRate) {
            onSecondaryCustomUpdate?.Invoke();
            secondaryCustomUpdateTimer = 0;
        }
    }

    private void OnApplicationQuit() {
        if (isLocal)
            return;

        _connectionManager.CloseConnection();
    }

    private void GetSceneManager() {
        _sceneManager = FindObjectOfType<SceneManager>();
        _sceneManager?.Init();
    }
}