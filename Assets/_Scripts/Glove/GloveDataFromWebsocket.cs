using System;
using System.Collections;
using UnityEngine;

public class GloveDataFromWebsocket : MonoBehaviour
{
    public string WebsocketUri = "ws://localhost:3000";

    [Header("Debug")] public bool OutputData;

    public static Quaternion Rotation;
    public static float Jaw;

    private string _data;
    private string[] _dataArr;

    private void OnEnable()
    {
        StartCoroutine(GrabData());
    }

    private IEnumerator GrabData()
    {
        var webSocket = new WebSocket(new Uri(WebsocketUri));

        yield return StartCoroutine(webSocket.Connect());

        while (enabled)
        {
            if (webSocket.error != null)
            {
                Debug.LogError("Error: " + webSocket.error);
                break;
            }

            _data = webSocket.RecvString();
            if (!string.IsNullOrEmpty(_data))
            {
                if (_data.StartsWith("DEBUG"))
                {
                    Debug.Log(_data);
                }
                else
                {
                    _dataArr = _data.Split(',');

                    if (_dataArr[0] == "E")
                    {
                        Rotation = Quaternion.Euler(
                            float.Parse(_dataArr[2]) * -1,
                            float.Parse(_dataArr[1]),
                            float.Parse(_dataArr[3])
                        );

                        Jaw = int.Parse(_dataArr[4]);
                    }
                    else if (_dataArr[0] == "Q")
                    {
                        Rotation = new Quaternion(
                            float.Parse(_dataArr[1]),
                            float.Parse(_dataArr[2]),
                            float.Parse(_dataArr[3]),
                            float.Parse(_dataArr[4])
                        );

                        Jaw = int.Parse(_dataArr[5]);
                    }

                    if (OutputData)
                        Debug.Log($"Rotation: {Rotation} - Jaw: {Jaw}");
                }
            }

            yield return null;
        }

        webSocket.Close();
    }
}