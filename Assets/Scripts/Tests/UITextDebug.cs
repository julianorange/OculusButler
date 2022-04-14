using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITextDebug : MonoBehaviour
{

    public UITextDisplay butlerDisplay;
    public UITextDisplay playerDisplay;

    void Start() {

    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.A)) {
            butlerDisplay.DisplayText("Toto" + Random.Range(0, 100));
            Debug.Log("TOTO");
        }
        if (Input.GetKeyDown(KeyCode.Z)) {
            playerDisplay.DisplayText("Tutu" + Random.Range(0, 100));
            Debug.Log("TUTU");
        }
    }
}
