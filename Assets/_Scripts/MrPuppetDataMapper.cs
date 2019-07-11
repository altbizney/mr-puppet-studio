using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace MrPuppet
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

    public class MrPuppetDataMapper : MonoBehaviour
    {
        // yanked from Framer https://github.com/framer/Framer-fork/blob/master/framer/Utils.coffee#L285
        public float JawPercent => 0f + (((HubConnection.Jaw - JawClosed) / (float)(JawOpened - JawClosed)) * (1f - 0f));

        public enum Joint { Shoulder, Elbow, Wrist };

        public Transform ShoulderJoint { get; private set; }
        public Transform ElbowJoint { get; private set; }
        public Transform WristJoint { get; private set; }

        [Required]
        public MrPuppetHubConnection HubConnection;

        public Pose TPose;

        [Range(0f, 1023f)]
        public float JawOpened = 1023f;
        [Range(0f, 1023f)]
        public float JawClosed = 0f;

        [Range(.2f, 4f)]
        public float ArmLength = 1f;
        [Range(.2f, 4f)]
        public float ForearmLength = 1f;

        private void Awake()
        {
            ShoulderJoint = new GameObject("Shoulder").transform;
            ShoulderJoint.SetParent(transform, false);

            ElbowJoint = new GameObject("Elbow").transform;
            ElbowJoint.SetParent(ShoulderJoint);

            WristJoint = new GameObject("Wrist").transform;
            WristJoint.SetParent(ElbowJoint);
        }

        private void OnValidate()
        {
            if (HubConnection == null) HubConnection = FindObjectOfType<MrPuppetHubConnection>();
        }

        private void Update()
        {
            // apply rotations (subtracting TPose)
            ShoulderJoint.rotation = HubConnection.ShoulderRotation * Quaternion.Inverse(TPose.ShoulderRotation);

            ElbowJoint.localPosition = Vector3.back * ArmLength;
            ElbowJoint.rotation = HubConnection.ElbowRotation * Quaternion.Inverse(TPose.ElbowRotation);

            WristJoint.localPosition = Vector3.back * ForearmLength;
            WristJoint.rotation = HubConnection.WristRotation * Quaternion.Inverse(TPose.WristRotation);
        }

        public Transform GetJoint(Joint joint)
        {
            switch (joint)
            {
                case Joint.Shoulder:
                    return ShoulderJoint;
                case Joint.Elbow:
                    return ElbowJoint;
                case Joint.Wrist:
                    return WristJoint;
            }

            throw new ArgumentException("Invalid Joint");
        }

        [Button(ButtonSizes.Large)]
        [HorizontalGroup("TPose")]
        [GUIColor(0f, 1f, 0f)]
        [DisableIf("CaptureButtonsEnabled")]
        public void GrabTPose()
        {
            TPose = new Pose
            {
                ShoulderRotation = HubConnection.ShoulderRotation,
                ElbowRotation = HubConnection.ElbowRotation,
                WristRotation = HubConnection.WristRotation
            };
        }

        [Button(ButtonSizes.Large, Name = "Clear")]
        [HorizontalGroup("TPose", Width = .1f)]
        public void ClearTPose()
        {
            TPose = new Pose();
        }

        [Button(ButtonSizes.Large)]
        [HorizontalGroup("Jaw")]
        [GUIColor(0f, 1f, 0f)]
        [DisableIf("CaptureButtonsEnabled")]
        public void GrabJawOpened()
        {
            JawOpened = HubConnection.Jaw;
        }

        [Button(ButtonSizes.Large)]
        [HorizontalGroup("Jaw")]
        [GUIColor(0f, 1f, 0f)]
        [DisableIf("CaptureButtonsEnabled")]
        public void GrabJawClosed()
        {
            JawClosed = HubConnection.Jaw;
        }

        [Button(ButtonSizes.Large, Name = "Clear")]
        [HorizontalGroup("Jaw", Width = .1f)]
        public void ClearJaw()
        {
            JawClosed = 0f;
            JawOpened = 1023f;
        }

        private bool CaptureButtonsEnabled()
        {
            return !Application.isPlaying;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            // body
            Gizmos.color = Color.grey;
            Gizmos.DrawWireCube(new Vector3(0f, -0.5f, 0.5f), new Vector3(0.5f, 1.5f, 1f));
            Gizmos.DrawWireCube(new Vector3(0f, 0.4f, 0.5f), Vector3.one * 0.33f);

            Gizmos.color = Color.white;

            // shoulder
            Gizmos.matrix = ShoulderJoint.localToWorldMatrix;
            Gizmos.DrawWireCube(new Vector3(0f, 0f, ArmLength * -0.5f), new Vector3(0.25f, 0.25f, ArmLength));

            // eblow
            Gizmos.matrix = ElbowJoint.localToWorldMatrix;
            Gizmos.DrawWireCube(new Vector3(0f, 0f, ForearmLength * -0.5f), new Vector3(0.25f, 0.25f, ForearmLength));

            // jaw
            Gizmos.matrix = Matrix4x4.TRS(WristJoint.position, WristJoint.rotation * Quaternion.Euler(Mathf.Lerp(0f, 45f, JawPercent) * 0.5f, 0f, 0f), Vector3.one);
            Gizmos.DrawWireCube(new Vector3(0f, 0.0625f, -0.25f), new Vector3(0.25f, 0.125f, 0.5f));
            Gizmos.matrix = Matrix4x4.TRS(WristJoint.position, WristJoint.rotation * Quaternion.Euler(Mathf.Lerp(0f, 45f, JawPercent) * -0.5f, 0f, 0f), Vector3.one);
            Gizmos.DrawWireCube(new Vector3(0f, -0.0625f, -0.25f), new Vector3(0.25f, 0.125f, 0.5f));

            Gizmos.matrix = Matrix4x4.identity;

            // axises
            Gizmos.color = Color.green;
            Gizmos.DrawRay(ShoulderJoint.position, ShoulderJoint.up * 0.25f);
            Gizmos.DrawRay(ElbowJoint.position, ElbowJoint.up * 0.25f);
            Gizmos.DrawRay(WristJoint.position, WristJoint.up * 0.25f);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(ShoulderJoint.position, ShoulderJoint.right * 0.25f);
            Gizmos.DrawRay(ElbowJoint.position, ElbowJoint.right * 0.25f);
            Gizmos.DrawRay(WristJoint.position, WristJoint.right * 0.25f);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(ShoulderJoint.position, ShoulderJoint.forward * 0.25f);
            Gizmos.DrawRay(ElbowJoint.position, ElbowJoint.forward * 0.25f);
            Gizmos.DrawRay(WristJoint.position, WristJoint.forward * 0.25f);
        }


        // The section below is used to store the changes made at runtime 
        static MrPuppetDataMapper()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private const string TPoseShoulderRotationKey = "shoulderRotationTPose";
        private const string TPoseElbowRotationKey = "elbowRotationTPose";
        private const string TPoseWristRotationKey = "wristRotationTPose";
        private const string JawClosedKey = "jawClosed";
        private const string JawOpenedKey = "jawOpened";
        private const string ArmLengthKey = "armLength";
        private const string ForearmLengthKey = "forearmLength";

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            var dataMapper = FindObjectOfType<MrPuppetDataMapper>();
            if (dataMapper == null) return;

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                Undo.RecordObject(dataMapper, "Undo DataMapper");
                dataMapper.TPose = new Pose()
                {
                    ShoulderRotation = PlayerPrefsX.GetQuaternion(TPoseShoulderRotationKey),
                    ElbowRotation = PlayerPrefsX.GetQuaternion(TPoseElbowRotationKey),
                    WristRotation = PlayerPrefsX.GetQuaternion(TPoseWristRotationKey)
                };

                dataMapper.JawOpened = PlayerPrefs.GetFloat(JawOpenedKey);
                dataMapper.JawClosed = PlayerPrefs.GetFloat(JawClosedKey);

                dataMapper.ArmLength = PlayerPrefs.GetFloat(ArmLengthKey);
                dataMapper.ForearmLength = PlayerPrefs.GetFloat(ForearmLengthKey);
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                PlayerPrefsX.SetQuaternion(TPoseShoulderRotationKey, dataMapper.TPose.ShoulderRotation);
                PlayerPrefsX.SetQuaternion(TPoseElbowRotationKey, dataMapper.TPose.ElbowRotation);
                PlayerPrefsX.SetQuaternion(TPoseWristRotationKey, dataMapper.TPose.WristRotation);

                PlayerPrefs.SetFloat(JawOpenedKey, dataMapper.JawOpened);
                PlayerPrefs.SetFloat(JawClosedKey, dataMapper.JawClosed);

                PlayerPrefs.SetFloat(ArmLengthKey, dataMapper.ArmLength);
                PlayerPrefs.SetFloat(ForearmLengthKey, dataMapper.ForearmLength);
            }
        }
    }
}