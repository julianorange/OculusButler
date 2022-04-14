using OculusSampleFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{

    public OVRHand rHand;
    [SerializeField] private ButlerObjectSelectorSO objectSelector;
    [SerializeField] private float defaultObjectDistance = 5;
    [SerializeField] private bool isGhost;
    [SerializeField] private float maxDistanceForward;
    [SerializeField] private float minDistanceBackward;
    [SerializeField] private float moveSpeed;

    private MeshRenderer meshRenderer;
    private bool indexPinchTriggered;
    private bool moveForward;

    void Start() {
        transform.position = rHand.PointerPose.position + (defaultObjectDistance * rHand.transform.forward);
        meshRenderer = GetComponent<MeshRenderer>();
        indexPinchTriggered = false;
        moveForward = true;
    }

    void Update() {

        bool isIndexPinching = rHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        OVRHand.TrackingConfidence indexConfidence = rHand.GetFingerConfidence(OVRHand.HandFinger.Index);
        if (indexConfidence == OVRHand.TrackingConfidence.High && isIndexPinching) {
            indexPinchTriggered = !indexPinchTriggered;
        }

        bool isMovementActive = isGhost || indexPinchTriggered;

        if (rHand.IsPointerPoseValid && isMovementActive) {
            transform.position = rHand.PointerPose.position + (defaultObjectDistance * rHand.PointerPose.forward);
            transform.LookAt(rHand.transform.position);
            meshRenderer.enabled = true;

            bool isMiddlePinching = rHand.GetFingerIsPinching(OVRHand.HandFinger.Middle);
            OVRHand.TrackingConfidence middleConfidence = rHand.GetFingerConfidence(OVRHand.HandFinger.Middle);
            if (middleConfidence == OVRHand.TrackingConfidence.High && isMiddlePinching) {
                //Frontal move
                //float cameraToObjectDistance = (objectSelector.objectData.position - Camera.current.transform.position).magnitude;
                float cameraToObjectDistance = (objectSelector.theObject.transform.position - Camera.current.transform.position).magnitude;
                if (cameraToObjectDistance > maxDistanceForward)
                    moveForward = false;
                else if (cameraToObjectDistance < minDistanceBackward)
                    moveForward = true;

                Vector3 moveDirection = Camera.current.transform.forward;
                moveDirection.y = 0;
                if (!moveForward)
                    moveDirection = -moveDirection;
                //objectSelector.objectData.position += (moveDirection * moveSpeed * Time.deltaTime);
                objectSelector.theObject.transform.position += (moveDirection * moveSpeed * Time.deltaTime);
            }
        } else {
            meshRenderer.enabled = false;
        }
    }
}
