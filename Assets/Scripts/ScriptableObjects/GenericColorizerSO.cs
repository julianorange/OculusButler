using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GenericColorizer", menuName = "ScriptableObjects/ButlerObjectColorizer/GenericColorizer", order = 1)]
public class GenericColorizerSO : ButlerColorizerSO {
    public override void Colorize(GameObject obj, Color color) {
        obj.GetComponent<MeshRenderer>().material.color = color;
    }

    public override Color GetCurrentColor(GameObject obj) {
        return obj.GetComponent<MeshRenderer>().material.color;
    }
}
