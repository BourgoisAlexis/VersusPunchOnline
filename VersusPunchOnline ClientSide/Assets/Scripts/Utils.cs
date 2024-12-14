using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using PlayerIOClient;

public static class Utils {
    #region Simple Logs
    public static void Log(string message) {
        Debug.Log($"{message}");
    }

    public static void Log(string source, string methodName, string message = "") {
        Debug.Log($"{source} > {methodName} > {message}");
    }

    public static void Log(object source, string methodName, string message = "") {
        Log(source.GetType().ToString(), methodName, message);
    }
    #endregion

    #region Error Logs
    public static void LogError(string message) {
        Debug.LogError($"{message}");
    }

    public static void LogError(string source, string methodName, string message = "") {
        Debug.LogError($"{source} > {methodName} > {message}");
    }

    public static void LogError(object source, string methodName, string message = "") {
        LogError(source.GetType().ToString(), methodName, message);
    }

    public static void ErrorOnParams(object source, string methodName) {
        LogError(source, methodName, "Error on parameters");
    }
    #endregion

    #region Player.IO
    public static string[] GetMessageParams(Message m) {
        List<string> infos = new List<string>();
        for (int i = 0; i < m.Count; i++)
            infos.Add(m[(uint)i].ToString());

        return infos.ToArray();
    }

    public static void LogMessage(Message m) {
        StringBuilder b = new StringBuilder();
        b.Append($"{m.Type} > ");
        string[] infos = GetMessageParams(m);
        foreach (string info in infos)
            b.AppendLine(info);

        Utils.Log("CommonUtils", "LogMessage", b.ToString());
    }
    #endregion


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

        GlobalManager.Instance.NavigationManager.AutoClearingActionOnLoad(onLoad);
    }
}