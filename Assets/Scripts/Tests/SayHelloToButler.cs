using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class SayHelloToButler : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            StartCoroutine(Upload());
    }

    IEnumerator Upload()
    {

        //string json = "{ \"kind\" :\"formula\", \"content\" :\"(I Julian (B self (greeting-from-to thierry self)))\"}";
        string json = "{\"kind\" :\"utterance\", \"author\":\"thierry\", \"content\" :\"ça va ?\"}";

        UnityWebRequest www = new UnityWebRequest("http://localhost:3001/api/v1/events", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Authorization", "Basic " + System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes("butler:.Butler0")));
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
        }
    }
}