using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ButlerObjectCreatorSO : ScriptableObject
{

    public abstract GameObject InstantiateObject(ButlerManager.ButlerEvent butlerEvent, ButlerObjectSO butlerObject, ButlerManager butlerManager, int id, Transform parent, Vector3? position, Vector3 rotation);
    public GameObject InstantiateGhost(ButlerManager.ButlerEvent butlerEvent, ButlerObjectSO butlerObject, int id, Transform parent) {
        GameObject clone = Instantiate<GameObject>(butlerObject.ghostPrefab);
        clone.name = butlerObject.idName + id.ToString() + " Ghost";
        clone.transform.parent = parent;
        return clone;
    }
}
