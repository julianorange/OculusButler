using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ButlerColor", menuName = "ScriptableObjects/ButlerColor")]
public class ButlerColorSO : ScriptableObject
{
    public string idName;
    public string[] texts;
    public Color color;
}
