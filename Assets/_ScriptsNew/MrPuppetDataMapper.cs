using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Thinko.MrPuppet
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
        public Pose FinalPose
        {
            get
            {
                _finalPose.Set(
                    HubConnection.ShoulderRotation * Quaternion.Inverse(TPose.ShoulderRotation),
                    HubConnection.ElbowRotation * Quaternion.Inverse(TPose.ElbowRotation),
                    HubConnection.WristRotation * Quaternion.Inverse(TPose.WristRotation));
                return _finalPose;
            }
        }

        // yanked from Framer https://github.com/framer/Framer-fork/blob/master/framer/Utils.coffee#L285
        public float JawPercent => 0f + (((HubConnection.Jaw - JawClosed) / (float)(JawOpened - JawClosed)) * (1f - 0f));

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

        [Range(.2f, .4f)]
        public float ArmLength = .3f;
        [Range(.2f, .4f)]
        public float ForearmLength = .3f;
        
        private Pose _finalPose;

        private void Awake()
        {
            _finalPose = new Pose();
            
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
            var finalPose = FinalPose;

            // Position and Rotate the joints
            ShoulderJoint.rotation = finalPose.ShoulderRotation;
            ElbowJoint.rotation = finalPose.ElbowRotation;
            ElbowJoint.localPosition = Vector3.right * ArmLength;
            WristJoint.rotation = finalPose.WristRotation;
            WristJoint.localPosition = Vector3.right * ForearmLength;
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

        [Button(ButtonSizes.Small, Name = "Clear")]
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

        [Button(ButtonSizes.Small, Name = "Clear")]
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