using OculusSampleFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonListener : MonoBehaviour {

    public Material contactMaterial;
    public GameObject[] smallCubes;
    private Material defaultMaterial;
    void Start() {
        GetComponent<ButtonController>().InteractableStateChanged.AddListener(InitiateEvent);
        defaultMaterial = GetComponent<MeshRenderer>().material;
    }

    private void InitiateEvent(InteractableStateArgs state) {

        if (state.NewInteractableState == InteractableState.ProximityState) {
        } else if (state.NewInteractableState == InteractableState.ContactState) {
            GetComponent<MeshRenderer>().material = contactMaterial;
        } else if (state.NewInteractableState == InteractableState.ActionState) {
            foreach (GameObject cube in smallCubes) {
                cube.SetActive(!cube.activeInHierarchy);
            }
        } else {
            GetComponent<MeshRenderer>().material = defaultMaterial;
        }
    }

    // Update is called once per frame
    void Update() {

    }
}
