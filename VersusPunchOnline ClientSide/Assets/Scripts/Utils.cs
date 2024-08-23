using System;
using System.Collections.Generic;
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
        int r = UnityEngine.Random.Range(0, 255);
        return (float)r / 255f;
    }

    /// <summary>
    /// Add actions to customUpdate and remove them on scene load
    /// </summary>
    /// <param name="actions"></param>
    public static void AutoClearingActionOnCustomUpdate(params Action[] actions) {
        NavigationManager nav = GlobalManager.Instance.NavigationManager;

        foreach (Action action in actions)
            GlobalManager.Instance.onCustomUpdate += action;

        Action onLoad = () => {
            foreach (Action action in actions)
                GlobalManager.Instance.onCustomUpdate -= action;
        };

        Action onLoaded = () => {
            nav.onLoadScene -= onLoad;
        };

        nav.onLoadScene += onLoad;
        nav.onSceneLoaded += onLoaded;
    }

    /// <summary>
    /// Add actions to loadScene and remove them on scene loaded
    /// </summary>
    /// <param name="actions"></param>
    public static void AutoClearingActionOnLoad(params Action[] actions) {
        NavigationManager nav = GlobalManager.Instance.NavigationManager;

        foreach (Action action in actions)
            nav.onLoadScene += action;

        Action onLoaded = () => {
            foreach (Action action in actions)
                nav.onLoadScene -= action;
        };

        nav.onSceneLoaded += onLoaded;
    }
}