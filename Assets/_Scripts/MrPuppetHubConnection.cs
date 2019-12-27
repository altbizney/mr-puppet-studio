using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif

namespace MrPuppet
{
    [Serializable]
    public class SensorCalibrationData
    {
        [ReadOnly, GUIColor("SystemColor")]
        public int System;

        [ReadOnly, GUIColor("GyroColor")]
        public int Gyro;

        [ReadOnly, GUIColor("AccelerometerColor")]
        public int Accelerometer;

        [ReadOnly, GUIColor("MagnetometerColor")]
        public int Magnetometer;

        public void Set(int system, int gyro, int accelerometer, int magnetometer)
        {
            System = system;
            Gyro = gyro;
            Accelerometer = accelerometer;
            Magnetometer = magnetometer;
        }

        public Color SystemColor => GetColor(System);
        public Color GyroColor => GetColor(Gyro);
        public Color AccelerometerColor => GetColor(Accelerometer);
        public Color MagnetometerColor => GetColor(Magnetometer);

        public Color GetColor(int code)
        {
            switch (code)
            {
                case 0:
                    return Color.red;
                case 3:
                    return Color.green;
                default:
                    return Color.yellow;
            }
        }

        public bool IsCalibrated => System == 3 && Gyro == 3 && Accelerometer == 3 && Magnetometer == 3;
    }

    public class MrPuppetHubConnection : MonoBehaviour
    {
        private MrPuppetDataMapper DataMapper;

        [Header("Connection")]
        public string WebsocketUri = "ws://localhost:3000";

        [MinValue(0.1f), SuffixLabel("seconds")]
        public float ReconnectInterval = 2f;

        [ReadOnly]
        public bool IsConnected = false;

        [Header("Adjustments")]
        [ToggleLeft, Tooltip("Fixes quaternion coordinate space (BNO055's right-handed → Unity's left-handed)")]
        public bool FixRightHandedQuaternions = true;

        [ToggleLeft, Tooltip("Fixes the orentation not matching Unity's Transform space")]
        public bool FixSwappedOrientation = true;

        private Quaternion FixSwappedOrientationQuaternion = Quaternion.Euler(0, 90f, -180f);

        [Header("Sensors")]
        [ReadOnly]
        public Quaternion WristRotation;

        [ReadOnly]
        public Quaternion ElbowRotation;

        [ReadOnly]
        public Quaternion ShoulderRotation;

        [ReadOnly]
        public float Jaw;

        [Header("Calibration")]
        [InlineProperty, LabelText("Wrist")]
        public SensorCalibrationData WristCalibrationData;
        [InlineProperty, LabelText("Elbow"), Space]
        public SensorCalibrationData ElbowCalibrationData;
        [InlineProperty, LabelText("Shoulder"), Space]
        public SensorCalibrationData ShoulderCalibrationData;

        [Header("Debug")]
        [ToggleLeft]
        public bool OutputData = false;

        private WebSocket webSocket;

        private string _data;
        private string[] _array;
        private string[] _wrist;
        private string[] _elbow;
        private string[] _shoulder;

        private float _lastUpdateTime;

        private void Awake()
        {
            DataMapper = FindObjectOfType<MrPuppetDataMapper>();

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
                        if (_data.StartsWith("DEBUG"))
                        {
                            // output debug
                            Debug.Log(_data);
                        }
                        else if (_data.StartsWith("COMMAND"))
                        {
                            IsConnected = true;

                            // process packet
                            _array = _data.Split(';');

                            if (_array.Length < 2)
                            {
                                Debug.LogWarning("[COMMAND] UNKNOWN: " + _data);
                                return;
                            }

                            switch (_array[1])
                            {
                                case "JAW_OPENED":
                                    Debug.Log("[COMMAND] JawOpened = " + _array[2]);
                                    DataMapper.JawOpened = float.Parse(_array[2]);
                                    break;
                                case "JAW_CLOSED":
                                    Debug.Log("[COMMAND] JawClosed = " + _array[2]);
                                    DataMapper.JawClosed = float.Parse(_array[2]);
                                    break;
                                case "TPOSE":
                                    Debug.Log("[COMMAND] TPOSE updated");
                                    DataMapper.TPose.FromString(_array[2].Split(','), _array[3].Split(','), _array[4].Split(','));
                                    break;
                                case "ATTACH":
                                    // TODO: support for multiple puppets
                                    Debug.Log("[COMMAND] ATTACH updated");
                                    FindObjectOfType<ButtPuppet>().AttachPoseFromString(_array[2].Split(','), _array[3].Split(','), _array[4].Split(','));
                                    break;
                                default:
                                    Debug.LogWarning("[COMMAND] UNKNOWN: " + _data);
                                    break;
                            }

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

                            if (FixSwappedOrientation)
                            {
                                ShoulderRotation *= FixSwappedOrientationQuaternion;
                                ElbowRotation *= FixSwappedOrientationQuaternion;
                                WristRotation *= FixSwappedOrientationQuaternion;
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
}
