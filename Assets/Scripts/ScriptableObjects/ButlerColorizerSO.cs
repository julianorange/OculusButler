using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ButlerColorizerSO : ScriptableObject
{
    public abstract void Colorize(GameObject obj, Color color);
    public abstract Color GetCurrentColor(GameObject obj);
}
