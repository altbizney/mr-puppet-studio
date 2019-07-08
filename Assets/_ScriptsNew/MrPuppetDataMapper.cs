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
        public MrPuppetHubConnection HubConnection;

        public Pose TPose;
        
        public int JawOpened = 1023;
        public int JawClosed = 0;
        
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
            JawClosed = 0;
            JawOpened = 1023;
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

                dataMapper.JawOpened = PlayerPrefs.GetInt(JawOpenedKey);
                dataMapper.JawClosed = PlayerPrefs.GetInt(JawClosedKey);
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                PlayerPrefsX.SetQuaternion(TPoseShoulderRotationKey, dataMapper.TPose.ShoulderRotation);
                PlayerPrefsX.SetQuaternion(TPoseElbowRotationKey, dataMapper.TPose.ElbowRotation);
                PlayerPrefsX.SetQuaternion(TPoseWristRotationKey, dataMapper.TPose.WristRotation);
                
                PlayerPrefs.SetInt(JawOpenedKey, dataMapper.JawOpened);
                PlayerPrefs.SetInt(JawClosedKey, dataMapper.JawClosed);
            }
        }
    }
}