using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButlerCurrentLocationVisualizer : MonoBehaviour {

    public ButlerManager butlerManager;
    public GameObject particlePrefab;

    private Vector3 position;
    private Vector3 prevPosition;

    private GameObject particleClone;

    void Start() {
        position = Vector3.zero;
        prevPosition = position;
    }

    void Update() {

        if (butlerManager.HasCurrentLocationFact()) {
            position = butlerManager.GetCurrentLocationFact();
            if (position != prevPosition) {
                Destroy(particleClone);
                particleClone = Instantiate<GameObject>(particlePrefab, position, Quaternion.identity);
                particleClone.transform.parent = transform;
            }
            prevPosition = position;
        } else if (particleClone != null) {
            Destroy(particleClone);
        }
    }
}
