using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif

namespace MrPuppet
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
        [Header("Hub")]
        public string WebsocketUri = "ws://localhost:3000";

        [MinValue(0.1f)]
        public float ReconnectInterval = 2f;

        [ReadOnly]
        public bool IsConnected = false;

        [Header("Adjustments")]
        [Tooltip("Fixes quaternion coordinate space (BNO055's right-handed → Unity's left-handed)")]
        public bool FixRightHandedQuaternions = true;

        [Tooltip("Fixes the orentation not matching Unity's Transform space")]
        public bool FixSwappedOrientation = true;

        // [Header("Legacy Adjustments")]
        // [Tooltip("Fixes the elbow sensor being mounted upside down (relative to the shoulder/wrist)")]
        // public bool FixInvertedElbowSensor = false;

        // [Tooltip("Fixes the X and Y axises being flipped")]
        // public bool FixSwappedXYAxis = false;

        [ReadOnly, Header("Sensors")]
        public Quaternion WristRotation;

        [ReadOnly]
        public Quaternion ElbowRotation;

        [ReadOnly]
        public Quaternion ShoulderRotation;

        [ReadOnly]
        public float Jaw;

        [Header("Debug")]
        public bool OutputData = false;

        private WebSocket webSocket;

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

        public void SendSocketMessage(string message)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("Called SendMessage while not connected: " + message);
                return;
            }

            try
            {
                webSocket.SendString(message);
            }
            catch (System.FormatException e)
            {
                Debug.Log(e.Message);
            }
        }

        private IEnumerator GrabData()
        {
            webSocket = new WebSocket(new Uri(WebsocketUri));

            yield return StartCoroutine(webSocket.Connect());

            while (enabled)
            {
                if (webSocket.error != null)
                {
                    IsConnected = false;
                    Debug.LogError("[WS] " + webSocket.error + " Reconnecting in " + ReconnectInterval + " sec...");
                    // TODO: show an on screen message / reconnect countdown
                    yield return new WaitForSecondsRealtime(ReconnectInterval);
                    StartCoroutine(GrabData());
                    break;
                }

                try
                {
                    _data = webSocket.RecvString();
                    if (!string.IsNullOrEmpty(_data))
                    {
                        if (_data.StartsWith("DEBUG") || _data.StartsWith("COMMAND"))
                        {
                            // output debug
                            Debug.Log(_data);
                        }
                        else
                        {
                            IsConnected = true;

                            // process packet
                            _array = _data.Split(';');

                            // jaw
                            Jaw = float.Parse(_array[0]);

                            // rotations
                            _wrist = _array[1].Split(',');
                            _elbow = _array[2].Split(',');
                            _shoulder = _array[3].Split(',');

                            // initialize raw quaternions
                            if (FixRightHandedQuaternions)
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

                            // if (FixSwappedXYAxis)
                            // {
                            //     ShoulderRotation *= Quaternion.Euler(0, -90f, 0);
                            //     ElbowRotation *= Quaternion.Euler(0, -90f, 0);
                            //     WristRotation *= Quaternion.Euler(0, -90f, 0);
                            // }

                            // if (FixInvertedElbowSensor)
                            // {
                            //     ElbowRotation *= Quaternion.Euler(0, 180f, 0);
                            // }

                            if (FixSwappedOrientation)
                            {
                                ShoulderRotation *= Quaternion.Euler(0, 90f, -180f);
                                ElbowRotation *= Quaternion.Euler(0, 90f, -180f);
                                WristRotation *= Quaternion.Euler(0, 90f, -180f);
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
                }
                catch (System.FormatException e)
                {
                    Debug.Log(e.Message);
                }

                yield return null;
            }

            webSocket.Close();
        }
    }

#if UNITY_EDITOR
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
#endif
}