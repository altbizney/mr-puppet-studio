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
        
        [Button(ButtonSizes.Large)]
        [HorizontalGroup("TPose")]
        [GUIColor(0f, 1f, 0f)]
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
        
        // The section below is solely to keep the changes made during play time 
        static MrPuppetDataMapper()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private const string TPoseShoulderRotationKey = "shoulderRotationTPose";
        private const string TPoseElbowRotationKey = "elbowRotationTPose";
        private const string TPoseWristRotationKey = "wristRotationTPose";
        
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
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                PlayerPrefsX.SetQuaternion(TPoseShoulderRotationKey, dataMapper.TPose.ShoulderRotation);
                PlayerPrefsX.SetQuaternion(TPoseElbowRotationKey, dataMapper.TPose.ElbowRotation);
                PlayerPrefsX.SetQuaternion(TPoseWristRotationKey, dataMapper.TPose.WristRotation);
            }
        }
    }
}