using UnityEngine;

public static class Utils {
    public static void Log(object source, string prefix, string message = "", bool lowPriority = false) {
        if (lowPriority && !GlobalManager.Instance.showLowPriorityLogs)
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