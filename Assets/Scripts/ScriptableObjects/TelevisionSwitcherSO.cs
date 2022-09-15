using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LightSwitcher", menuName = "ScriptableObjects/ButlerObjectSwitcher/TelevisionSwitcher")]
public class TelevisionSwitcherSO : ButlerSwitcherSO
{
    public override bool GetSwitchState(GameObject obj) {
        return obj.GetComponent<TelevisionController>().GetSwitchState();
    }

    public override void Switch(GameObject obj, bool switchStatus) {
        obj.GetComponent<TelevisionController>().Switch(switchStatus);
    }
}
