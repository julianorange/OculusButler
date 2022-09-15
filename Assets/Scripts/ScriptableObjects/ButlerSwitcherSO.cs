using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ButlerSwitcherSO : ScriptableObject
{
    public abstract void Switch(GameObject obj, bool switchStatus);
    public abstract bool GetSwitchState(GameObject obj);
}
