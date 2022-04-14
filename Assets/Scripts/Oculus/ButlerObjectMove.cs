using OculusSampleFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButlerObjectMove : MonoBehaviour
{

    public OVRHand rHand;
    public OVRHand lHand;
    [SerializeField] private ButlerObjectSelectorSO objectSelector;
    [SerializeField] private float defaultObjectDistance = 5;
    [SerializeField] private bool isGhost;
    [SerializeField] private float maxDistanceForward;
    [SerializeField] private float minDistanceBackward;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;

    private bool indexPinchTriggered;
    private bool indexPinchLocked;
    private float handToObjectDistance;
    private int moveDirection;

    //private ButlerObjectDataSO previousData;
    private GameObject previousObject;

    void Start() {
        Init();
    }

    void Update() {


        if (objectSelector.theObject != null) {

            if (previousObject != objectSelector.theObject) {
                if (isGhost) {
                    handToObjectDistance = defaultObjectDistance;
                } else {
                    handToObjectDistance = (rHand.PointerPose.position - objectSelector.theObject.transform.position).magnitude;
                }
            }

            //if index finger is pinched, activate/deactivate the tracking of the right hand position by the selected object
            bool isRightIndexPinching = rHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
            OVRHand.TrackingConfidence indexConfidence = rHand.GetFingerConfidence(OVRHand.HandFinger.Index);
            if (indexConfidence == OVRHand.TrackingConfidence.High && isRightIndexPinching) {
                if (!indexPinchLocked) {
                    indexPinchTriggered = !indexPinchTriggered;
                    indexPinchLocked = true;
                }
            } else {
                indexPinchLocked = false;
            }

            bool isMovementActive = isGhost || indexPinchTriggered;

            if (rHand.IsPointerPoseValid && isMovementActive) {

                //Disable all physical colliders to be able to move the object freely in the environment
                EnablePhysicalColliders(objectSelector.theObject, false);

                //if left middle finger is pinching, change the distance between the hand and the selected object
                bool isLeftMiddlePinching = lHand.GetFingerIsPinching(OVRHand.HandFinger.Middle);
                OVRHand.TrackingConfidence middleConfidence = lHand.GetFingerConfidence(OVRHand.HandFinger.Middle);
                if (middleConfidence == OVRHand.TrackingConfidence.High && isLeftMiddlePinching) {
                    if (handToObjectDistance > maxDistanceForward)
                        moveDirection = -1;
                    else if (handToObjectDistance < minDistanceBackward)
                        moveDirection = 1;
                    handToObjectDistance += moveDirection * moveSpeed * Time.deltaTime;
                }

                //Follow the hand movement
                objectSelector.theObject.transform.position = rHand.PointerPose.position + (handToObjectDistance * rHand.PointerPose.forward);

                //if left finger is pinching, rotate the selected object
                bool isLeftRingPinching = lHand.GetFingerIsPinching(OVRHand.HandFinger.Ring);
                OVRHand.TrackingConfidence ringConfidence = lHand.GetFingerConfidence(OVRHand.HandFinger.Ring);
                if (ringConfidence == OVRHand.TrackingConfidence.High && isLeftRingPinching) {
                    Vector3 eulerAngles = objectSelector.theObject.transform.eulerAngles;
                    objectSelector.theObject.transform.eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y + rotationSpeed * Time.deltaTime, eulerAngles.z);
                }
            } else {//i.e., the selected object being not null, if (!rHand.IsPointerPoseValid || !isMovementInactive)
                EnablePhysicalColliders(objectSelector.theObject, true);
            }
        } else if (previousObject != null) {//We're just getting out from an interaction phase, make sure all colliders are re-enabled
            EnablePhysicalColliders(previousObject, true);
        }

        previousObject = objectSelector.theObject;
    }

    private void EnablePhysicalColliders(GameObject obj, bool enable) {

        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders) {
            if (!collider.isTrigger)
                collider.enabled = enable;
        }
    }

    public void Init() {
        indexPinchTriggered = false;
        indexPinchLocked = false;
        handToObjectDistance = defaultObjectDistance;
        previousObject = null;
        moveDirection = 1;
    }

    public void SetIsGhost(bool b) {
        isGhost = b;
    }
}
