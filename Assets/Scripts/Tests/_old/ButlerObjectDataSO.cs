using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ButlerColor", menuName = "ScriptableObjects/ObjectData")]
public class ButlerObjectDataSO : ScriptableObject
{
    public void Init(Vector3 _position, Vector3 _rotation, Color _color) {
        position = _position;
        rotation = _rotation;
        color = _color;
    }

    public Vector3 position;
    public Vector3 rotation;
    public Color color;
}
