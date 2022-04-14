using Facebook.WitAi.Lib;
using Oculus.Voice;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VoiceActivator : MonoBehaviour
{

    public OVRHand lHand;
    public AppVoiceExperience appVoiceExperience;
    public AudioSource audioSource;
    public ButlerManager butlerManager;

    public AudioClip startListeningClip;
    public AudioClip stopListeningClip;

    public UITextDisplay transcriptionText;

    // Start is called before the first frame update
    void Start() {
        //Debug.Log(Microphone.devices.Length);
        //Debug.Log(Application.internetReachability.ToString());

        appVoiceExperience.events.OnStartListening.AddListener(OnStartListening);
        appVoiceExperience.events.OnStoppedListening.AddListener(OnStoppedListening);
        appVoiceExperience.events.OnFullTranscription.AddListener(OnFullTranscription);
        appVoiceExperience.events.OnPartialTranscription.AddListener(OnPartialTranscription);
        appVoiceExperience.events.OnResponse.AddListener(OnResponse);
    }


    // Update is called once per frame
    void Update() {
        OVRInput.Update();

        //Vector3 lHandPos = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LHand);

        bool isIndexPinching = lHand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        //float strength = lHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        OVRHand.TrackingConfidence confidence = lHand.GetFingerConfidence(OVRHand.HandFinger.Index);

        if (Input.GetKeyDown(KeyCode.Space) || (confidence == OVRHand.TrackingConfidence.High && isIndexPinching)) {
            appVoiceExperience.Activate();
        }
    }

    private void OnStartListening() {

        audioSource.Stop();
        audioSource.clip = startListeningClip;
        audioSource.Play();
    }

    private void OnStoppedListening() {

        audioSource.Stop();
        audioSource.clip = stopListeningClip;
        audioSource.Play();
    }

    private void OnPartialTranscription(string partialTranscription) {
        transcriptionText.DisplayText(partialTranscription);
    }

    private void OnFullTranscription(string transcription) {

        butlerManager.SendUtteranceToButler(transcription);
    }
    private void OnResponse(WitResponseNode responseNode) {

        transcriptionText.EraseText();
    }

    private void FixedUpdate() {
        OVRInput.FixedUpdate();
    }
}
