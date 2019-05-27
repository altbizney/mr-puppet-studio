using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Thinko
{
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
        
        private Transform _shoulderJoint;
        private Transform _elbowJoint;
        private Transform _wristJoint;

        private void Awake()
        {
            _shoulderJoint = new GameObject("Shoulder").transform;
            _shoulderJoint.SetParent(transform, false);
            
            _elbowJoint = new GameObject("Elbow").transform;
            _elbowJoint.SetParent(_shoulderJoint);
            
            _wristJoint = new GameObject("Wrist").transform;
            _wristJoint.SetParent(_elbowJoint);
            
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
            _shoulderJoint.rotation = Quaternion.Slerp(
                _shoulderJoint.rotation,
                DataProvider.GetInput(RealPuppetDataProvider.Source.Shoulder) * Quaternion.Inverse(TPose.ShoulderRotation),
                Sharpness);
            
            _elbowJoint.rotation = Quaternion.Slerp(
                _elbowJoint.rotation,
                DataProvider.GetInput(RealPuppetDataProvider.Source.Elbow) * Quaternion.Inverse(TPose.ElbowRotation),
                Sharpness);
            
            _wristJoint.rotation = Quaternion.Slerp(
                _wristJoint.rotation,
                DataProvider.GetInput(RealPuppetDataProvider.Source.Wrist) * Quaternion.Inverse(TPose.WristRotation),
                Sharpness);
            
            // Calculate the final pose
            FinalPose.ShoulderRotation = AttachPose.ShoulderRotation * Quaternion.Inverse(_shoulderJoint.rotation);
            FinalPose.ElbowRotation = AttachPose.ElbowRotation * Quaternion.Inverse(_elbowJoint.rotation);
            FinalPose.WristRotation = AttachPose.WristRotation * Quaternion.Inverse(_wristJoint.rotation);
        }
        
        [Button(ButtonSizes.Large)]
        [HorizontalGroup("TPose")]
        [GUIColor(0f, 1f, 0f)]
        public void GrabTPose()
        {
            Debug.Log("Grabbed T-Pose");
            TPose = GrabPose();
        }
        
        [Button(ButtonSizes.Small, Name = "Clear")]
        [HorizontalGroup("TPose", Width = .1f)]
        public void ClearTPose()
        {
            TPose = new Pose();
        }
        
        [Button(ButtonSizes.Large)]
        [HorizontalGroup("AttachPose")]
        [GUIColor(0f, 1f, 0f)]
        public void GrabAttachPose()
        {
            Debug.Log("Grabbed Attach-Pose");
            AttachPose = GrabPose();
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
            if(_elbowJoint != null)
                _elbowJoint.localPosition = GetDirection(ArmDirection) * ShoulderLength;
            
            if(_wristJoint != null)
                _wristJoint.localPosition = GetDirection(ArmDirection) * ElbowLength;
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
            DrawJointDirectionGizmo(_shoulderJoint);
            DrawBoneGizmo(_shoulderJoint.position, _shoulderJoint.rotation, GetDirection(ArmDirection), ShoulderLength, Color.white);
            DrawJointDirectionGizmo(_elbowJoint);
            DrawBoneGizmo(_elbowJoint.position, _elbowJoint.rotation, GetDirection(ArmDirection), ElbowLength, Color.white);
            DrawJointDirectionGizmo(_wristJoint);
            DrawBoneGizmo(_wristJoint.position, _wristJoint.rotation, GetDirection(ArmDirection), WristLength, Color.white);
            
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

        private static RealBody.Pose _tPose;
        private static RealBody.Pose _attachedPose;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            _realBody = target as RealBody;
            
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
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

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.ExitingPlayMode)
            {
                _tPose = _realBody.TPose;
                _attachedPose = _realBody.AttachPose;
            }
            else if (obj == PlayModeStateChange.EnteredEditMode)
            {
                _realBody.TPose = _tPose;
                _realBody.AttachPose = _attachedPose;
            }
        }
    }
}