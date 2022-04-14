using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OculusSampleFramework;

public class OculusButlerFocusListener : MonoBehaviour
{

    public ButtonController oculusButtonController;
    public ButlerManager butlerManager;
    public ButlerObjectSelectorSO currentObjectSelector;

    void Start() {
        oculusButtonController.InteractableStateChanged.AddListener(InitiateEvent);
    }

    private void InitiateEvent(InteractableStateArgs state) {

        if (state.NewInteractableState == InteractableState.ProximityState) {
        } else if (state.NewInteractableState == InteractableState.ContactState) {
            butlerManager.SendToButlerFocusOn(gameObject.name);
            //currentObjectSelector.objectData = gameObject.GetComponent<UpdateObjectData>().GetButlerObjectData();
            currentObjectSelector.theObject = gameObject;
        } else if (state.NewInteractableState == InteractableState.ActionState) {
        } else {
            //currentObjectSelector.objectData = null;
            currentObjectSelector.theObject = null;
            butlerManager.SendToButlerFocusOn("none");
        }
    }

    void Update() {

    }
}
