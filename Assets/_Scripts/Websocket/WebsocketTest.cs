using System;
using System.Collections;
using UnityEngine;

public class WebsocketTest : MonoBehaviour
{
    public string WebsocketUri;
    private WebSocket _webSocket;

    private IEnumerator Start()
    {
        _webSocket = new WebSocket(new Uri(WebsocketUri));

        yield return StartCoroutine(_webSocket.Connect());

        while (true)
        {
            var reply = _webSocket.RecvString();

            if (reply != null)
            {
                Debug.Log(reply);
            }

            if (_webSocket.error != null)
            {
                Debug.LogError("Error: " + _webSocket.error);
                break;
            }

            yield return null;
        }

        _webSocket.Close();
    }

    public void Send(string data)
    {
        _webSocket.SendString(data);
    }
}