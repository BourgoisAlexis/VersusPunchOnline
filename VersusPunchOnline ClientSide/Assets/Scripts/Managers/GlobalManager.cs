using DPhysx;
using System;
using UnityEngine;

public class GlobalManager : MonoBehaviour {
    public static GlobalManager Instance;

    public NavigationManager navigationManager;
    public PlayerIOManager playerIOManager;
    public PeerToPeerManager<FrameData> peerToPeerManager;
    public InputManager inputManager { get; set; }
    public DPhysxManager dPhysxManager { get; set; }

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

    private void Awake() {
        if (Instance == null)
            Instance = this;

        navigationManager = new NavigationManager();
        playerIOManager = new PlayerIOManager();
        peerToPeerManager = new PeerToPeerManager<FrameData>();

        dPhysxManager = GetComponent<DPhysxManager>();
        inputManager = GetComponent<InputManager>();

        DontDestroyOnLoad(gameObject);
    }

    private void Update() {
        playerIOManager.ProcessMessages();
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

        peerToPeerManager.CloseConnection();
    }
}
