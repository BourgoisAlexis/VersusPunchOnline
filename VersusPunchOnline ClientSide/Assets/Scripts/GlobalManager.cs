using DPhysx;
using System;
using UnityEngine;

public class GlobalManager : MonoBehaviour {
    #region Variables
    public static GlobalManager Instance;

    private NavigationManager _navigationManager;
    private PlayerIOManager _playerIOManager;
    private UDPGameplay _connectionManager;
    private InputManager _inputManager;
    private DPhysxManager _dPhysxManager;

    [Header("Debug params")]
    public bool connectToLocal = true;
    public bool showLowPriority = true;
    public bool slowMode = true;
    public bool isLocal = true;

    private float customUpdateTimer = 0;
    private float secondaryCustomUpdateTimer = 0;
    public Action onCustomUpdate;
    public Action onSecondaryCustomUpdate;

    public int selfID = 0;

    //Accessors
    public NavigationManager NavigationManager => _navigationManager;
    public PlayerIOManager PlayerIOManager => _playerIOManager;
    public UDPGameplay ConnectionManager => _connectionManager;
    public InputManager InputManager => _inputManager;
    public DPhysxManager PhysicsManager => _dPhysxManager;
    #endregion


    private void Awake() {
        if (Instance == null)
            Instance = this;

        _navigationManager = new NavigationManager();
        _playerIOManager = new PlayerIOManager();
        _connectionManager = new UDPGameplay();

        _dPhysxManager = GetComponent<DPhysxManager>();
        _inputManager = GetComponent<InputManager>();

        DontDestroyOnLoad(gameObject);
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
}
