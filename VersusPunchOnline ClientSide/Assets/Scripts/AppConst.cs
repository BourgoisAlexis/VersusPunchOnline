using UnityEngine;

public static class AppConst {
    public const int synchroDuration = 180;
    public const int randomSeed = 7812;

    public const int inputDelay = 3;

    public const float customUpdateRate = 0.01667f;
    public const float secondaryCustomUpdateRate = 0.5f;

    public const string tagPlayer = "player";
    public const string tagHitBox = "hitBox";

    public const string defaultRoomID = "Lobby";
    public const int userLimitPerRoom = 30;

    public const int pollRate = 10;

    public const string versionKey = "version";
    public const string version = "0.0";

    //PlayerIO messages
    public const string serverMessageError = "servermessage_error";
    public const string serverMessageJoin = "servermessage_join";
    public const string serverMessageRequest = "servermessage_request";
    public const string serverMessageAskForConnectionInfos = "servermessage_askforconnectioninfos";
    public const string serverMessageConnectionInfos = "servermessage_connectionInfos";

    public const string userMessageRequestPTP = "usermessage_requestp2p";
    public const string userMessageAcceptRequest = "usermessage_acceptrequest";
    public const string userMessageDeclineRequest = "usermessage_declinerequest";
    public const string userMessageP2POpen = "usermessage_p2popen";

    //Colors
    public static Color HitBoxColor => Color.green;
    public static Color SpatialGridColor => Color.cyan;
    public static Color RigidBodyColor(DPhysx.DPhysxRigidbody rb) => rb.isTrigger ? Color.white : (rb.isStatic ? Color.red : Color.yellow);
}

public enum PlayerStates {
    Idle,
    Run,
    Midair,
    Punch,
    Dead,

    Default
}

public enum GameState {
    MainScreen,
    Gameplay,

    Default
}
