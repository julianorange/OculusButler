using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LightColorizer", menuName = "ScriptableObjects/ButlerObjectColorizer/LightColorizer")]
public class LightColorizerSO : ButlerColorizerSO
{
    public override void Colorize(GameObject obj, Color color) {
        obj.GetComponent<Light>().color = color;
    }

    public override Color GetCurrentColor(GameObject obj) {
        return obj.GetComponent<Light>().color;
    }
}
