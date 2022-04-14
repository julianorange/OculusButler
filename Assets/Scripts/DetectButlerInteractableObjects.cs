using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetectButlerInteractableObjects : MonoBehaviour {

    public Camera fpsCamera;
    public Image crosshair;
    public Image crosshairHover;
    public ButlerManager butlerManager;
    public float maxDistance = 50f;

    void Update() {
        Vector3 rayOrigin = fpsCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        LayerMask mask = LayerMask.GetMask("Butler Interactable");

        if (Physics.Raycast(rayOrigin, fpsCamera.transform.forward, out hit, maxDistance, mask)) {

            string objectName = hit.transform.gameObject.name;
            butlerManager.SendToButlerFocusOn(objectName);
            crosshair.gameObject.SetActive(false);
            crosshairHover.gameObject.SetActive(true);
        } else {
            butlerManager.SendToButlerFocusOn("none");
            crosshairHover.gameObject.SetActive(false);
            crosshair.gameObject.SetActive(true);
        }
    }
}
