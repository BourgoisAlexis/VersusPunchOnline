using UnityEngine;

public static class AppConst {
    public const int SYNCHRO_DURATION = 180;
    public const int RANDOM_SEED = 7812;

    public const int ONLINE_INPUT_DELAY = 3;

    public const float CUSTOM_UPDATE_RATE = 0.01667f;
    public const float SECONDARY_CUSTOM_UPDATE_RATE = 0.5f;

    public const string TAGP_LAYER = "player";
    public const string TAG_HITBOX = "hitBox";

    public const string DEFAULT_ROOM_ID = "Lobby";
    public const int USER_LIMIT_PER_ROOM = 30;

    public const int POLL_RATE = 10;

    public const string VERSION_KEY = "version";
    public const string VERSION = "0.0";


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
