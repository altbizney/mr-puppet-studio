﻿using System;
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
        
        public enum Direction
        {
            PositiveX,
            PositiveY,
            PositiveZ,
            NegativeX,
            NegativeY,
            NegativeZ
        }

        public Pose FinalPose { get; } = new Pose();

        [Required]
        public RealPuppetDataProvider DataProvider;

        public Direction ArmDirection = Direction.NegativeZ;

        [Range(0, 1)]
        public float ShoulderLength = 1f;
        [Range(0, 1)]
        public float ElbowLength = .75f;
        [Range(0, 1)]
        public float WristLength = .5f;

        [Range(0, 1)]
        public float Sharpness = .5f;

        [HorizontalGroup("TPose")]
        public Pose TPose = new Pose();
        
        [HorizontalGroup("AttachPose")]
        public Pose AttachPose = new Pose();

        public Transform ShoulderJoint { get; private set; }
        public Transform ElbowJoint { get; private set; }
        public Transform WristJoint { get; private set; }
        
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
                if(realBody== null) return;
                
                realBody.TPose = new Pose()
                {
                    ShoulderRotation = PlayerPrefsX.GetQuaternion(TPoseShoulderRotationKey),
                    ElbowRotation = PlayerPrefsX.GetQuaternion(TPoseElbowRotationKey),
                    WristRotation = PlayerPrefsX.GetQuaternion(TPoseWristRotationKey)
                };
                
                realBody.AttachPose = new Pose()
                {
                    ShoulderRotation = PlayerPrefsX.GetQuaternion(AttachPoseShoulderRotationKey),
                    ElbowRotation = PlayerPrefsX.GetQuaternion(AttachPoseElbowRotationKey),
                    WristRotation = PlayerPrefsX.GetQuaternion(AttachPoseWristRotationKey)
                };
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
        
        private void Update()
        {
            // Grab TPose
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.T))
                GrabTPose();
            
            // Grab AttachPose
            if (Input.GetKeyDown(KeyCode.A))
                GrabAttachPose();
            
            // Rotate the joints
            ShoulderJoint.rotation = Quaternion.Slerp(
                ShoulderJoint.rotation,
                DataProvider.GetInput(RealPuppetDataProvider.Source.Shoulder) * Quaternion.Inverse(TPose.ShoulderRotation),
                Sharpness);
            
            ElbowJoint.rotation = Quaternion.Slerp(
                ElbowJoint.rotation,
                DataProvider.GetInput(RealPuppetDataProvider.Source.Elbow) * Quaternion.Inverse(TPose.ElbowRotation),
                Sharpness);
            
            WristJoint.rotation = Quaternion.Slerp(
                WristJoint.rotation,
                DataProvider.GetInput(RealPuppetDataProvider.Source.Wrist) * Quaternion.Inverse(TPose.WristRotation),
                Sharpness);
            
            // Calculate the final pose
            FinalPose.ShoulderRotation = AttachPose.ShoulderRotation * Quaternion.Inverse(ShoulderJoint.rotation);
            FinalPose.ElbowRotation = AttachPose.ElbowRotation * Quaternion.Inverse(ElbowJoint.rotation);
            FinalPose.WristRotation = AttachPose.WristRotation * Quaternion.Inverse(WristJoint.rotation);
        }

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
        
        private const string AttachPoseShoulderRotationKey = "shoulderRotationAttachPose";
        private const string AttachPoseElbowRotationKey = "elbowRotationAttachPose";
        private const string AttachPoseWristRotationKey = "wristRotationAttachPose";
        
        [Button(ButtonSizes.Large)]
        [HorizontalGroup("AttachPose")]
        [GUIColor(0f, 1f, 0f)]
        public void GrabAttachPose()
        {
            var pose = GrabPose();
            AttachPose = pose;
            
            PlayerPrefsX.SetQuaternion(AttachPoseShoulderRotationKey, pose.ShoulderRotation);
            PlayerPrefsX.SetQuaternion(AttachPoseElbowRotationKey, pose.ElbowRotation);
            PlayerPrefsX.SetQuaternion(AttachPoseWristRotationKey, pose.WristRotation);
        }
        
        [Button(ButtonSizes.Small, Name = "Clear")]
        [HorizontalGroup("AttachPose", Width = .1f)]
        public void ClearAttachPose()
        {
            AttachPose = new Pose();
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

        private void AdjustJointsPositions()
        {
            if(ElbowJoint != null)
                ElbowJoint.localPosition = GetDirection(ArmDirection) * ShoulderLength;
            
            if(WristJoint != null)
                WristJoint.localPosition = GetDirection(ArmDirection) * ElbowLength;
        }

        private Vector3 GetDirection(Direction dir)
        {
            switch (dir)
            {
                case Direction.PositiveX:
                    return new Vector3(1, 0, 0);
                case Direction.PositiveY:
                    return new Vector3(0, 1, 0);
                case Direction.PositiveZ:
                    return new Vector3(0, 0, 1);
                case Direction.NegativeX:
                    return new Vector3(-1, 0, 0);
                case Direction.NegativeY:
                    return new Vector3(0, -1, 0);
                case Direction.NegativeZ:
                    return new Vector3(0, 0, -1);
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
            }
        }
        
        private void OnDrawGizmos()
        {
            if(!Application.isPlaying) return;
            
            // Draw the input bones
            DrawJointDirectionGizmo(ShoulderJoint);
            DrawBoneGizmo(ShoulderJoint.position, ShoulderJoint.rotation, GetDirection(ArmDirection), ShoulderLength, Color.white);
            DrawJointDirectionGizmo(ElbowJoint);
            DrawBoneGizmo(ElbowJoint.position, ElbowJoint.rotation, GetDirection(ArmDirection), ElbowLength, Color.white);
            DrawJointDirectionGizmo(WristJoint);
            DrawBoneGizmo(WristJoint.position, WristJoint.rotation, GetDirection(ArmDirection), WristLength, Color.white);
            
            void DrawJointDirectionGizmo(Transform transf)
            {
                var pos = transf.position;
                var rot = transf.rotation;
                var gizmoSize = HandleUtility.GetHandleSize(pos);
                Debug.DrawRay(pos, rot * transf.forward * gizmoSize, Color.blue, 0f, true);
                Debug.DrawRay(pos, rot * transf.up * gizmoSize, Color.green, 0f, true);
                Debug.DrawRay(pos, rot * transf.right * gizmoSize, Color.red, 0f, true);
            }
            
            void DrawBoneGizmo(Vector3 pos, Quaternion rot, Vector3 dir, float length, Color color)
            {
                Gizmos.DrawWireSphere(pos, HandleUtility.GetHandleSize(pos) * length * .1f);
                Debug.DrawRay(pos, rot * dir * length, color, 0, true);
            }
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

            // Calibration info
            if(!Application.isPlaying) return;
            var defColor = GUI.color;
            
            var calibData = _realBody.DataProvider.GetCalibrationData(RealPuppetDataProvider.Source.Shoulder);
            GUI.color = calibData.IsCalibrated ? Color.green : Color.yellow;
            GUILayout.Box($"Shoulder - System: {calibData.System}  Gyro: {calibData.Gyro}  Accl: {calibData.Accelerometer}  Mag:  {calibData.Magnetometer}");
            GUI.color = defColor;
            calibData = _realBody.DataProvider.GetCalibrationData(RealPuppetDataProvider.Source.Elbow);
            GUI.color = calibData.IsCalibrated ? Color.green : Color.yellow;
            GUILayout.Box($"Elbow - System: {calibData.System}  Gyro: {calibData.Gyro}  Accl: {calibData.Accelerometer}  Mag:  {calibData.Magnetometer}");
            GUI.color = defColor;
            calibData = _realBody.DataProvider.GetCalibrationData(RealPuppetDataProvider.Source.Wrist);
            GUI.color = calibData.IsCalibrated ? Color.green : Color.yellow;
            GUILayout.Box($"Wrist - System: {calibData.System}  Gyro: {calibData.Gyro}  Accl: {calibData.Accelerometer}  Mag:  {calibData.Magnetometer}");
            GUI.color = defColor;
        }
    }
}