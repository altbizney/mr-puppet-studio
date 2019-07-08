using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Thinko
{
    [InitializeOnLoadAttribute]
    public class RealBody : MonoBehaviour
    {
        [Serializable]
        public class Pose
        {
            public Quaternion ShoulderRotation = Quaternion.identity;
            public Quaternion ElbowRotation = Quaternion.identity;
            public Quaternion WristRotation = Quaternion.identity;

            public void Set(Quaternion shoulder, Quaternion elbow, Quaternion wrist)
            {
                ShoulderRotation = shoulder;
                ElbowRotation = elbow;
                WristRotation = wrist;
            }
        }

        public Pose FinalPose { get; } = new Pose();

        [Required]
        public RealPuppetDataProvider DataProvider;

        [Range(0, 1)]
        public float ShoulderLength = 1f;
        [Range(0, 1)]
        public float ElbowLength = .75f;

        [Range(0, 1)]
        public float Sharpness = .5f;

        [HorizontalGroup("TPose")]
        public Pose TPose = new Pose();

        [HorizontalGroup("JawClosed")]
        public float JawClosed = 0;

        [HorizontalGroup("JawOpened")]
        public float JawOpened = 1023;

        public float JawPercent = 0f;

        public Transform ShoulderJoint { get; private set; }
        public Transform ElbowJoint { get; private set; }
        public Transform WristJoint { get; private set; }

        public Vector3 OffsetRotation;

        static RealBody()
        {
            // We need this so we can keep the changes made during play time
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                var realBody = FindObjectOfType<RealBody>();
                if (realBody == null) return;

                realBody.TPose = new Pose()
                {
                    ShoulderRotation = PlayerPrefsX.GetQuaternion(TPoseShoulderRotationKey),
                    ElbowRotation = PlayerPrefsX.GetQuaternion(TPoseElbowRotationKey),
                    WristRotation = PlayerPrefsX.GetQuaternion(TPoseWristRotationKey)
                };

                realBody.JawClosed = PlayerPrefs.GetFloat(JawClosedKey);
                realBody.JawOpened = PlayerPrefs.GetFloat(JawOpenedKey);
            }
        }

        private void Awake()
        {
            ShoulderJoint = new GameObject("Shoulder").transform;
            ShoulderJoint.SetParent(transform, false);

            ElbowJoint = new GameObject("Elbow").transform;
            ElbowJoint.SetParent(ShoulderJoint);

            WristJoint = new GameObject("Wrist").transform;
            WristJoint.SetParent(ElbowJoint);

            AdjustJointsPositions();
        }

        private void OnValidate()
        {
            AdjustJointsPositions();
        }

        private void AdjustJointsPositions()
        {
            if (ElbowJoint != null)
                ElbowJoint.localPosition = Vector3.right * ShoulderLength;

            if (WristJoint != null)
                WristJoint.localPosition = Vector3.right * ElbowLength;
        }

        private void Update()
        {
            // Rotate the joints
            ShoulderJoint.rotation = Quaternion.Slerp(
                ShoulderJoint.rotation,
                DataProvider.GetInput(RealPuppetDataProvider.Source.Shoulder) * Quaternion.Inverse(TPose.ShoulderRotation) * Quaternion.Euler(OffsetRotation),
                Sharpness);

            ElbowJoint.rotation = Quaternion.Slerp(
                ElbowJoint.rotation,
                DataProvider.GetInput(RealPuppetDataProvider.Source.Elbow) * Quaternion.Inverse(TPose.ElbowRotation) * Quaternion.Euler(OffsetRotation),
                Sharpness);

            WristJoint.rotation = Quaternion.Slerp(
                WristJoint.rotation,
                DataProvider.GetInput(RealPuppetDataProvider.Source.Wrist) * Quaternion.Inverse(TPose.WristRotation) * Quaternion.Euler(OffsetRotation),
                Sharpness);

            // Calculate the final pose
            FinalPose.ShoulderRotation = ShoulderJoint.rotation;
            FinalPose.ElbowRotation = ElbowJoint.rotation;
            FinalPose.WristRotation = WristJoint.rotation;

            // Calculate jaw amount (unclamped 0-1)
            // TODO: move to e.g. DataMapper
            // yanked from Framer https://github.com/framer/Framer-fork/blob/master/framer/Utils.coffee#L285
            JawPercent = 0f + (((DataProvider.Jaw - JawClosed) / (JawOpened - JawClosed)) * (1f - 0f));
        }

        // TPose
        private const string TPoseShoulderRotationKey = "shoulderRotationTPose";
        private const string TPoseElbowRotationKey = "elbowRotationTPose";
        private const string TPoseWristRotationKey = "wristRotationTPose";

        [Button(ButtonSizes.Large)]
        [HorizontalGroup("TPose")]
        [GUIColor(0f, 1f, 0f)]
        public void GrabTPose()
        {
            var pose = GrabPose();
            TPose = pose;

            OffsetRotation = new Vector3(0, ShoulderJoint.rotation.eulerAngles.y, 0);

            PlayerPrefsX.SetQuaternion(TPoseShoulderRotationKey, pose.ShoulderRotation);
            PlayerPrefsX.SetQuaternion(TPoseElbowRotationKey, pose.ElbowRotation);
            PlayerPrefsX.SetQuaternion(TPoseWristRotationKey, pose.WristRotation);
        }

        [Button(ButtonSizes.Small, Name = "Clear")]
        [HorizontalGroup("TPose", Width = .1f)]
        public void ClearTPose()
        {
            TPose = new Pose();
        }

        private Pose GrabPose()
        {
            return new Pose
            {
                ShoulderRotation = DataProvider.GetInput(RealPuppetDataProvider.Source.Shoulder),
                ElbowRotation = DataProvider.GetInput(RealPuppetDataProvider.Source.Elbow),
                WristRotation = DataProvider.GetInput(RealPuppetDataProvider.Source.Wrist)
            };
        }

        // Jaw Closed
        private const string JawClosedKey = "jawClosed";

        [Button(ButtonSizes.Large)]
        [HorizontalGroup("JawClosed")]
        [GUIColor(0f, 1f, 0f)]
        public void GrabJawClosed()
        {
            JawClosed = DataProvider.Jaw;
            PlayerPrefs.SetFloat(JawClosedKey, JawClosed);
        }

        // Jaw Opened
        private const string JawOpenedKey = "jawOpened";

        [Button(ButtonSizes.Large)]
        [HorizontalGroup("JawOpened")]
        [GUIColor(0f, 1f, 0f)]
        public void GrabJawOpened()
        {
            JawOpened = DataProvider.Jaw;
            PlayerPrefs.SetFloat(JawOpenedKey, JawOpened);
        }
    }

    [CustomEditor(typeof(RealBody))]
    public class RealBodyEditor : OdinEditor
    {
        private RealBody _realBody;

        protected override void OnEnable()
        {
            base.OnEnable();

            _realBody = target as RealBody;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!Application.isPlaying) return;
            var defColor = GUI.color;

            // Calibration info
            GUILayout.Space(10);
            GUILayout.Label("CALIBRATION", EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
            var calibData = _realBody.DataProvider.GetSensorCalibrationData(RealPuppetDataProvider.Source.Shoulder);
            GUI.color = calibData.IsCalibrated ? Color.green : Color.yellow;
            GUILayout.Box($"Shoulder - System: {calibData.System}  Gyro: {calibData.Gyro}  Accl: {calibData.Accelerometer}  Mag:  {calibData.Magnetometer}");
            GUI.color = defColor;
            calibData = _realBody.DataProvider.GetSensorCalibrationData(RealPuppetDataProvider.Source.Elbow);
            GUI.color = calibData.IsCalibrated ? Color.green : Color.yellow;
            GUILayout.Box($"Elbow - System: {calibData.System}  Gyro: {calibData.Gyro}  Accl: {calibData.Accelerometer}  Mag:  {calibData.Magnetometer}");
            GUI.color = defColor;
            calibData = _realBody.DataProvider.GetSensorCalibrationData(RealPuppetDataProvider.Source.Wrist);
            GUI.color = calibData.IsCalibrated ? Color.green : Color.yellow;
            GUILayout.Box($"Wrist - System: {calibData.System}  Gyro: {calibData.Gyro}  Accl: {calibData.Accelerometer}  Mag:  {calibData.Magnetometer}");
            GUI.color = defColor;
        }
    }
}