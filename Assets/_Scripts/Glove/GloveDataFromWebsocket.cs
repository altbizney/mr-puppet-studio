using System;
using System.Collections;
using Thinko;
using UnityEngine;

namespace Thinko
{
    public class GloveDataFromWebsocket : RealPuppetDataProvider
    {
        public string WebsocketUri = "ws://localhost:3000";

        [Header("Debug")] public bool OutputData;

        private string _data;
        private string[] _dataArr;

        private Quaternion Rotation_curr = Quaternion.identity;
        private Quaternion Rotation2_curr = Quaternion.identity;

        private Quaternion Rotation_calib = Quaternion.identity;
        private Quaternion Rotation2_calib = Quaternion.identity;

        private void OnEnable()
        {
            StartCoroutine(GrabData());
        }

        private void Update(){
            if (Input.GetKeyDown("space"))
            {
                Rotation_calib = Rotation_curr;
                Rotation2_calib = Rotation2_curr;
            }
        }


        private Quaternion lhq(string data0,string data1,string data2,string data3) {
            // The BNO055 sends a right handed quaternion in (w, x, y, z) format.
            // Unity is a left handed quaternion in the (x, y, z, w) format.
            // Quaternion q_lefthanded = new Quaternion( -bno[3], -bno[1], -bno[2], bno[0] );
            // https://forums.adafruit.com/viewtopic.php?f=8&t=81671

            return new Quaternion(
                            float.Parse(data3) * -1,
                            float.Parse(data1) * -1,
                            float.Parse(data2) * -1,
                            float.Parse(data0)
                        );
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

                        Rotation_curr = lhq(_dataArr[0],_dataArr[1],_dataArr[2],_dataArr[3]);
                        Rotation2_curr = lhq(_dataArr[4],_dataArr[5],_dataArr[6],_dataArr[7]);

                        Rotation = Rotation_curr * Quaternion.Inverse(Rotation_calib);
                        Rotation2 = Rotation2_curr * Quaternion.Inverse(Rotation2_calib);

                        Jaw = int.Parse(_dataArr[8]);

                        if (OutputData)
                            Debug.Log($"Rotation: {Rotation} Rotation2: {Rotation2} Jaw: {Jaw}");
                    }
                }

                yield return null;
            }

            webSocket.Close();
        }
    }
}