using System;
using System.Collections;
using UnityEngine;

namespace Thinko
{
    public class WebsocketDataStream : RealPuppetDataProvider
    {
        public string WebsocketUri = "ws://localhost:3000";

        [Header("Debug")] public bool OutputData;

        private string _data;
        private string[] _array;
        private string[] _wrist;
        private string[] _elbow;
        private string[] _shoulder;

        private Quaternion WristRotation_curr = Quaternion.identity;
        private Quaternion WristRotation_calib = Quaternion.identity;

        private Quaternion ElbowRotation_curr = Quaternion.identity;
        private Quaternion ElbowRotation_calib = Quaternion.identity;

        private Quaternion ShoulderRotation_curr = Quaternion.identity;
        private Quaternion ShoulderRotation_calib = Quaternion.identity;


        private void OnEnable()
        {
            StartCoroutine(GrabData());
        }

        private void Update(){
            if (Input.GetKeyDown("space"))
            {
                WristRotation_calib = WristRotation_curr;
                ElbowRotation_calib = ElbowRotation_curr;
                ShoulderRotation_calib = ShoulderRotation_curr;
            }
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
                        WristRotation_curr = new Quaternion(float.Parse(_wrist[0]),float.Parse(_wrist[1]),float.Parse(_wrist[2]),float.Parse(_wrist[3]));
                        ElbowRotation_curr = new Quaternion(float.Parse(_elbow[0]),float.Parse(_elbow[1]),float.Parse(_elbow[2]),float.Parse(_elbow[3]));
                        ShoulderRotation_curr = new Quaternion(float.Parse(_shoulder[0]),float.Parse(_shoulder[1]),float.Parse(_shoulder[2]),float.Parse(_shoulder[3]));
                        
                        // grab calibration info
                        WristCalibrationData.Set(int.Parse(_wrist[4]), int.Parse(_wrist[5]), int.Parse(_wrist[6]), int.Parse(_wrist[7]));
                        ElbowCalibrationData.Set(int.Parse(_elbow[4]), int.Parse(_elbow[5]), int.Parse(_elbow[6]), int.Parse(_elbow[7]));
                        ShoulderCalibrationData.Set(int.Parse(_shoulder[4]), int.Parse(_shoulder[5]), int.Parse(_shoulder[6]), int.Parse(_shoulder[7]));

                        // apply calibration
                        WristRotation = WristRotation_curr * Quaternion.Inverse(WristRotation_calib);
                        ElbowRotation = ElbowRotation_curr * Quaternion.Inverse(ElbowRotation_calib);
                        ShoulderRotation = ShoulderRotation_curr * Quaternion.Inverse(ShoulderRotation_calib);

                        if (OutputData) {
                            Debug.Log($"Jaw: {Jaw} WristRotation: {WristRotation} ElbowRotation: {ElbowRotation} ShoulderRotation: {ShoulderRotation}");
                        }
                    }
                }

                yield return null;
            }

            webSocket.Close();
        }
    }
}