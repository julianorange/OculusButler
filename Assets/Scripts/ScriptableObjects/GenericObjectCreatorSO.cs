using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GenericCreator", menuName = "ScriptableObjects/ButlerObjectCreator/GenericCreator", order = 1)]
public class GenericObjectCreatorSO : ButlerObjectCreatorSO
{
    public override GameObject InstantiateObject(ButlerManager.ButlerEvent butlerEvent, ButlerObjectSO butlerObject, ButlerManager butlerManager, int id, Transform parent, Vector3? position, Vector3 rotation) {
        GameObject clone = Instantiate<GameObject>(butlerObject.prefab);
        clone.name = butlerObject.idName + id.ToString();
        clone.transform.parent = parent;
        if (position.HasValue) {
            clone.transform.position = position.Value;
        } else {
            clone.transform.position = ButlerObjectManager.ButlerToUnityPosition(butlerEvent.location.x, butlerEvent.location.y, butlerEvent.location.z);
        }
        clone.transform.eulerAngles = rotation;
        clone.GetComponent<OculusButlerFocusListener>().butlerManager = butlerManager;
        return clone;
    }
}
