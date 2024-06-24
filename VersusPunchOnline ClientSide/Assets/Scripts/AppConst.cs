using UnityEngine;

public static class AppConst {
    public const int inputDelay = 3;

    public const float customUpdateRate = 0.01667f;
    public const float secondaryCustomUpdateRate = 0.1f;

    public const string tagPlayer = "player";
    public const string tagHitBox = "hitBox";

    public const string defaultRoomID = "Lobby";
    public const int userLimitPerRoom = 30;

    public const string serverMessageError = "servermessage_error";
    public const string serverMessageJoin = "servermessage_join";
    public const string serverMessageRequest = "servermessage_request";
    public const string serverMessageAskForConnectionInfos = "servermessage_askforconnectioninfos";
    public const string serverMessageConnectionInfos = "servermessage_connectionInfos";

    public const string userMessageRequestPTP = "usermessage_requestptp";
    public const string userMessageAcceptRequest = "usermessage_acceptrequest";
    public const string userMessageDeclineRequest = "usermessage_declinerequest";
    public const string userMessagePTPOpen = "usermessage_ptpopen";
}

public static class Utils {
    public static void Log(object source, string prefix, string message = "", bool lowPriority = false) {
        if (lowPriority && !GlobalManager.Instance.showLowPriority)
            return;

        Debug.Log($"{source.GetType()} > {prefix} : {message}");
    }

    public static void LogError(object source, string prefix, string message = "") {
        Debug.LogError($"{source.GetType()} > {prefix} : {message}");
    }

    public static Color RandomColor() {
        return new Color(R(), R(), R(), 1);
    }

    public static float R() {
        int r = Random.Range(0, 255);
        return (float)r / 255f;
    }
}
