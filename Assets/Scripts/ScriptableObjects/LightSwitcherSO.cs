using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LightSwitcher", menuName = "ScriptableObjects/ButlerObjectSwitcher/LightSwitcher")]
public class LightSwitcherSO : ButlerSwitcherSO
{
    public override void Switch(GameObject obj, bool switchStatus) {
        obj.GetComponent<Light>().enabled = switchStatus;
    }
    public override bool GetSwitchState(GameObject obj) {
        return obj.GetComponent<Light>().enabled;
    }
}
