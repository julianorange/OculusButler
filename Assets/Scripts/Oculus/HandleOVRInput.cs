using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleOVRInput : MonoBehaviour {

    public OVRHand lHand;
    public OVRHand rHand;

    void Start() {

    }

    void Update() {
        OVRInput.Update();

        Vector3 lHandPos = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand);
        Vector3 rHandPos = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RHand);

        //Debug.Log("leftHand = " + lHandPos + ", rightHand = " + rHandPos);

        bool isIndexPinching = lHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        float strength = lHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        OVRHand.TrackingConfidence confidence = lHand.GetFingerConfidence(OVRHand.HandFinger.Index);

        Debug.Log("is left Pinching = " + isIndexPinching + ", strength = " + strength + "\t confidence = " + confidence);
    }

    private void FixedUpdate() {
        OVRInput.FixedUpdate();
    }
}
