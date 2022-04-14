using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dpoch.SocketIO;

public class ReceiveFromButler : MonoBehaviour
{
    void Start()
    {
        SocketIO socket = new SocketIO("ws://127.0.0.1:3001/socket.io/?EIO=4&transport=websocket");

        socket.OnOpen += () => Debug.Log("Socket open!");

        socket.On("socket id", (ev) =>
        {
            socket.Emit("socket id", "thierry");
            Debug.Log("socket id message received");
        });

        socket.On("event", (ev) =>
        {
            string myString = ev.Data[0].ToObject<string>();
            Debug.Log(myString);
        });

        socket.OnConnectFailed += () => Debug.Log("Socket failed to connect!");
        socket.OnClose += () => Debug.Log("Socket closed!");
        socket.OnError += (err) => Debug.Log("Socket Error: " + err);

        socket.Connect();

    }
}
