using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Text;
using System.Collections;

public class ButlerHttpLocationTracker : MonoBehaviour {


    public OVRHand oculusRightHand;

    private HttpListener listener;
    private Vector3 pointingPosition;

    void Start() {
        listener = new HttpListener();
        //listener.Prefixes.Add("http://" + ButlerManager.BUTLER_IP + ":3004/");
        listener.Prefixes.Add("http://*:3004/");
        listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
        listener.Start();

        StartCoroutine(StartListener());
    }

    void Update() {

        //pointingPosition = fpsCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f)) + (fpsCamera.transform.forward * 5);
        pointingPosition = oculusRightHand.PointerPose.transform.position + (oculusRightHand.PointerPose.transform.forward * 5);
    }

    private IEnumerator StartListener() {
        while (true) {
            var result = listener.BeginGetContext(ListenerCallback, listener);
            yield return new WaitUntil(() => result.IsCompleted);
        }
    }

    private void ListenerCallback(IAsyncResult result) {
        //Debug.Log("Calling unity server...");
        HttpListenerContext context = listener.EndGetContext(result);
        //Debug.Log("############################");
        //Debug.Log("Method is " + context.Request.HttpMethod);
        //Debug.Log("URL is " + context.Request.Url);
        if (context.Request.HttpMethod == "GET") {
            if (context.Request.Url.LocalPath == "/getLocation") {
                HttpListenerResponse response = context.Response;

                string posX = pointingPosition.x.ToString().Replace(",", ".");
                string posY = pointingPosition.y.ToString().Replace(",", ".");
                string posZ = pointingPosition.z.ToString().Replace(",", ".");

                string responseString = "{\"x\":\"" + posX + "\",\"y\":\"" + posY + "\",\"z\":\"" + posZ + "\"}";
                Debug.Log("Butler asking for position :" + responseString);
                byte[] buffer = Encoding.UTF8.GetBytes(responseString);

                response.ContentLength64 = buffer.Length;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);

                output.Close();
            }
        }
        context.Response.Close();
    }

    private void OnApplicationQuit() {
        listener.Stop();
        listener.Close();
    }

}