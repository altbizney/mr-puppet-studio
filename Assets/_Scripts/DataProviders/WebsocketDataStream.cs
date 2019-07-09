using System;
using System.Collections;
using UnityEngine;

namespace Thinko
{
    public class WebsocketDataStream : RealPuppetDataProvider
    {
        public string WebsocketUri = "ws://localhost:3000";

        [Header("Debug")]
        public bool OutputData = false;
        public bool FixQuaternions = true;

        private string _data;
        private string[] _array;
        private string[] _wrist;
        private string[] _elbow;
        private string[] _shoulder;

        private float _lastUpdateTime;


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
                    Debug.LogError("[Websocket] " + webSocket.error);
                    break;
                }

                _data = webSocket.RecvString();
                if (!string.IsNullOrEmpty(_data))
                {
                    if (_data.StartsWith("DEBUG"))
                    {
                        // output debug
                        Debug.Log(_data);
                    }
                    else
                    {
                        // process packet
                        _array = _data.Split(';');

                        // jaw
                        Jaw = int.Parse(_array[0]);

                        // rotations
                        _wrist = _array[1].Split(',');
                        _elbow = _array[2].Split(',');
                        _shoulder = _array[3].Split(',');

                        // initialize raw quaternions
                        if (FixQuaternions)
                        {
                            WristRotation = new Quaternion(-float.Parse(_wrist[3]), -float.Parse(_wrist[1]), -float.Parse(_wrist[2]), float.Parse(_wrist[0]));
                            ElbowRotation = new Quaternion(-float.Parse(_elbow[3]), -float.Parse(_elbow[1]), -float.Parse(_elbow[2]), float.Parse(_elbow[0]));
                            ShoulderRotation = new Quaternion(-float.Parse(_shoulder[3]), -float.Parse(_shoulder[1]), -float.Parse(_shoulder[2]), float.Parse(_shoulder[0]));
                        }
                        else
                        {
                            WristRotation = new Quaternion(float.Parse(_wrist[0]), float.Parse(_wrist[1]), float.Parse(_wrist[2]), float.Parse(_wrist[3]));
                            ElbowRotation = new Quaternion(float.Parse(_elbow[0]), float.Parse(_elbow[1]), float.Parse(_elbow[2]), float.Parse(_elbow[3]));
                            ShoulderRotation = new Quaternion(float.Parse(_shoulder[0]), float.Parse(_shoulder[1]), float.Parse(_shoulder[2]), float.Parse(_shoulder[3]));
                        }

                        // swap x/z
                        ShoulderRotation *= Quaternion.Euler(0, -90f, 0);
                        ElbowRotation *= Quaternion.Euler(0, -90f, 0);
                        WristRotation *= Quaternion.Euler(0, -90f, 0);

                        // elbow sensor is mounted backwards
                        ElbowRotation *= Quaternion.Euler(0, 180f, 0);

                        // grab calibration info
                        WristCalibrationData.Set(int.Parse(_wrist[4]), int.Parse(_wrist[5]), int.Parse(_wrist[6]), int.Parse(_wrist[7]));
                        ElbowCalibrationData.Set(int.Parse(_elbow[4]), int.Parse(_elbow[5]), int.Parse(_elbow[6]), int.Parse(_elbow[7]));
                        ShoulderCalibrationData.Set(int.Parse(_shoulder[4]), int.Parse(_shoulder[5]), int.Parse(_shoulder[6]), int.Parse(_shoulder[7]));

                        if (OutputData)
                        {
                            Debug.Log($"{(Time.time - _lastUpdateTime).ToString($"0.00")} - Jaw: {Jaw} WristRotation: {WristRotation.eulerAngles} ElbowRotation: {ElbowRotation.eulerAngles} ShoulderRotation: {ShoulderRotation.eulerAngles}");
                            _lastUpdateTime = Time.time;
                        }
                    }
                }

                yield return null;
            }

            webSocket.Close();
        }
    }
}