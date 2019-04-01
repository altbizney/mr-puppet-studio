using System;
using System.Collections;
using UnityEngine;

public class ReadRotationFromWebsocket : MonoBehaviour
{
    public string WebsocketUri;
    public Transform RotationTarget;
    [Range(0,1)]
    public float RotationSharpness = 1;

    public Vector3 RotationOffset;

    private IEnumerator Start()
    {
        var webSocket = new WebSocket(new Uri(WebsocketUri));

        yield return StartCoroutine(webSocket.Connect());

        while (true)
        {
            var reply = webSocket.RecvString();
            if (reply != null)
            {
                var input = reply.Split(',');
                var x = float.Parse(input[1]);
                var y = float.Parse(input[2]);
                var z = float.Parse(input[3]);
                var w = float.Parse(input[4]);
                var rotation = new Quaternion(x, y, z, w);

                RotationTarget.rotation = Quaternion.Lerp(RotationTarget.rotation, rotation * Quaternion.Euler(RotationOffset), RotationSharpness);
            }

            if (webSocket.error != null)
            {
                Debug.LogError("Error: " + webSocket.error);
                break;
            }

            yield return null;
        }

        webSocket.Close();
    }
}