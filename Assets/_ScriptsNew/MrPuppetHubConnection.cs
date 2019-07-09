using System;
using System.Collections;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Thinko.MrPuppet
{
    public class SensorCalibrationData
    {
        public int System;
        public int Gyro;
        public int Accelerometer;
        public int Magnetometer;

        public void Set(int system, int gyro, int accelerometer, int magnetometer)
        {
            System = system;
            Gyro = gyro;
            Accelerometer = accelerometer;
            Magnetometer = magnetometer;
        }

        public bool IsCalibrated => System == 3 && Gyro == 3 && Accelerometer == 3 && Magnetometer == 3;
    }

    public class MrPuppetHubConnection : MonoBehaviour
    {
        public string WebsocketUri = "ws://localhost:3000";
        public bool FixQuaternions = true;

        [ReadOnly]
        public Quaternion WristRotation;

        [ReadOnly]
        public Quaternion ElbowRotation;

        [ReadOnly]
        public Quaternion ShoulderRotation;

        [ReadOnly]
        public float Jaw;

        [Header("Debug")]
        public bool OutputData = false;

        private string _data;
        private string[] _array;
        private string[] _wrist;
        private string[] _elbow;
        private string[] _shoulder;

        private float _lastUpdateTime;

        public SensorCalibrationData WristCalibrationData;
        public SensorCalibrationData ElbowCalibrationData;
        public SensorCalibrationData ShoulderCalibrationData;

        private void Awake()
        {
            WristCalibrationData = new SensorCalibrationData();
            ElbowCalibrationData = new SensorCalibrationData();
            ShoulderCalibrationData = new SensorCalibrationData();
        }

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
                        Jaw = float.Parse(_array[0]);

                        // rotations
                        _wrist = _array[1].Split(',');
                        _elbow = _array[2].Split(',');
                        _shoulder = _array[3].Split(',');

                        // initialize raw quaternions
                        if (FixQuaternions) // fixes quaternion coordinate space (BNO055 → Unity)
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

    [CustomEditor(typeof(MrPuppetHubConnection))]
    public class MrPuppetHubConnectionEditor : OdinEditor
    {
        private MrPuppetHubConnection _hubConnection;

        protected override void OnEnable()
        {
            base.OnEnable();

            _hubConnection = target as MrPuppetHubConnection;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!Application.isPlaying) return;
            var defColor = GUI.color;

            // Calibration info
            GUILayout.Space(10);
            GUILayout.Label("CALIBRATION", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
            var calibData = _hubConnection.ShoulderCalibrationData;
            GUI.color = calibData.IsCalibrated ? Color.green : Color.yellow;
            GUILayout.Box($"Shoulder - {calibData.System}:{calibData.Gyro}:{calibData.Accelerometer}:{calibData.Magnetometer}");
            GUI.color = defColor;
            calibData = _hubConnection.ElbowCalibrationData;
            GUI.color = calibData.IsCalibrated ? Color.green : Color.yellow;
            GUILayout.Box($"Elbow - {calibData.System}:{calibData.Gyro}:{calibData.Accelerometer}:{calibData.Magnetometer}");
            GUI.color = defColor;
            calibData = _hubConnection.WristCalibrationData;
            GUI.color = calibData.IsCalibrated ? Color.green : Color.yellow;
            GUILayout.Box($"Wrist - {calibData.System}:{calibData.Gyro}:{calibData.Accelerometer}:{calibData.Magnetometer}");
            GUI.color = defColor;
        }
    }
}