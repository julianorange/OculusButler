using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugConsoleToUIText : MonoBehaviour {
    public Text text;
    public int maxCharacterToDisplay;
    private string output;
    static string myLog = "";
    private string stack;

    void OnEnable() {
        Application.logMessageReceived += Log;
    }

    void OnDisable() {
        Application.logMessageReceived -= Log;
    }

    private void Update() {
        text.text = myLog;
    }

    public void Log(string logString, string stackTrace, LogType type) {
        output = logString;
        stack = stackTrace;
        myLog = output + "\n" + myLog;
        if (type == LogType.Exception) {
            myLog = output + "\n" + stackTrace;
        }
        if (myLog.Length > maxCharacterToDisplay) {
            myLog = myLog.Substring(0, maxCharacterToDisplay);
        }
    }
}
