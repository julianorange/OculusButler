using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Dpoch.SocketIO;

public class Butler : MonoBehaviour
{
    public string userName;
    private SocketIO socket;
    private bool socketAvailable;
    private bool messageSent;

    void Start()
    {
        socket = new SocketIO("ws://127.0.0.1:3001/socket.io/?EIO=4&transport=websocket");

        socket.OnOpen += () => Debug.Log("Socket " + userName + " open!");

        socket.On("socket id", (ev) =>
        {
            socket.Emit("socket id", userName);
            Debug.Log("socket id message received for " + userName + ", emitting response.");
            socketAvailable = true;
        });

        socket.On("event", (ev) =>
        {
            string myString = ev.Data[0].ToObject<string>();
            Debug.Log("message pour " + userName + " : " + myString);
        });
        socket.OnConnectFailed += () => Debug.Log("Socket " + userName + " failed to connect!");
        socket.OnClose += () =>
        {
            socketAvailable = false;
            messageSent = false;
            Debug.Log("Socket " + userName + " closed!");
        };
        socket.OnError += (err) => Debug.Log("Socket " + userName + " Error: " + err);

        messageSent = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            ConnectSocket(socket);

        if (socketAvailable && !messageSent)
        {
            StartCoroutine(Upload(userName, socket));
            messageSent = true;
        }
    }

    private void ConnectSocket(SocketIO socket)
    {
        socket.Connect();
    }

    IEnumerator Upload(string userName, SocketIO socket)
    {
        //string json = "{ \"kind\" :\"formula\", \"content\" :\"(I Julian (B self (greeting-from-to thierry self)))\"}";
        string json = "{\"kind\" :\"utterance\", \"author\":\"" + userName + "\", \"content\" :\"ça va ?\"}";

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
            Debug.Log(userName + " a demandé au butler comment ça allait.");
        }
    }
}