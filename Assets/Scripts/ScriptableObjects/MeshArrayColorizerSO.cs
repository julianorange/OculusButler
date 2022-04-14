using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MeshArrayColorizer", menuName = "ScriptableObjects/ButlerObjectColorizer/MeshArrayColorizer", order = 1)]
public class MeshArrayColorizerSO : ButlerColorizerSO
{
    public Mesh[] meshArray;

    public override void Colorize(GameObject obj, Color color) {

        //Coloring the object itself if applicable
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter != null && Array.Exists(meshArray, element => element == meshFilter.sharedMesh)) {
            obj.GetComponent<MeshRenderer>().material.color = color;
        }
        //Coloring the relevant children
        foreach (Transform child in obj.GetComponentsInChildren<Transform>()) {
            meshFilter = child.GetComponent<MeshFilter>();
            if (meshFilter != null && Array.Exists(meshArray, element => element == meshFilter.sharedMesh)) {
                child.GetComponent<MeshRenderer>().material.color = color;
            }
        }

    }

    public override Color GetCurrentColor(GameObject obj) {

        Color res = Color.white;
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        if (meshFilter != null && Array.Exists(meshArray, element => element == meshFilter.sharedMesh)) {
            res = obj.GetComponent<MeshRenderer>().material.color;
        } else {
            bool found = false;
            int i = 0;
            Transform[] nestedChildren = obj.GetComponentsInChildren<Transform>();
            while (!found && i < nestedChildren.Length) {
                Transform child = nestedChildren[i];
                meshFilter = child.GetComponent<MeshFilter>();
                if (meshFilter != null && Array.Exists(meshArray, element => element == meshFilter.sharedMesh)) {
                    res = child.GetComponent<MeshRenderer>().material.color;
                    found = true;
                }
                i++;
            }
        }
        return res;
    }
}
