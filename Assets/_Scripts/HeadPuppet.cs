using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;

namespace MrPuppet
{
    public class HeadPuppet : MonoBehaviour
    {
        private MrPuppetDataMapper DataMapper;
        private MrPuppetHubConnection HubConnection;

        private Vector3 RootSpawnPosition;
        private Quaternion RootSpawnRotation;

        public Transform Root;

        private float LerpTimer;
        private bool DeattachPoseSet;
        private Quaternion RootRotationTarget;
        private Quaternion RotationModifiedTarget;

        [MinValue(0f)]
        public float RotationSpeed = 7f;
        [MinValue(0f)]
        public float PositionSpeed = 0.1f;
        private Vector3 PositionVelocity;

        [HorizontalGroup("RootExtentX")]
        public bool LimitRootExtentX = false;
        [HorizontalGroup("RootExtentX")]
        [ShowIf("LimitRootExtentX")]
        public float RootExtentX = 0f;

        [HorizontalGroup("RootExtentY")]
        public bool LimitRootExtentY = false;
        [HorizontalGroup("RootExtentY")]
        [ShowIf("LimitRootExtentY")]
        public float RootExtentY = 0f;

        [HorizontalGroup("RootExtentZ")]
        public bool LimitRootExtentZ = false;
        [HorizontalGroup("RootExtentZ")]
        [ShowIf("LimitRootExtentZ")]
        public float RootExtentZ = 0f;

        [Range(1f, 2.5f)]
        public float RotationModifier = 1f;

        [Range(1f, 4f)]
        public float AttachAmountDuration = 1f;
        private float AttachAmountValue;

        [Button(ButtonSizes.Large)]
        [GUIColor(0f, 1f, 0f)]
        [DisableInEditorMode()]
        public void FocusDataMapper()
        {
            Selection.activeGameObject = DataMapper.gameObject;
        }

        [Button(ButtonSizes.Large)]
        [GUIColor(0f, 1f, 0f)]
        [DisableInEditorMode()]
        public void TestDeattach()
        {
            LerpTimer = 0;
            DeattachPoseSet = true;
            AttachAmountValue = 0;
            RootRotationTarget = RotationDeltaFromAttachWrist();
        }

        //AttachAmountDuration, AttachAmountValue

        // public string AttachPoseToString()
        // {
        //     string packet = "";

        //     packet += AttachPoseWristPosition.x + "," + AttachPoseWristPosition.y + "," + AttachPoseWristPosition.z + ";";
        //     packet += AttachPoseWristRotation.x + "," + AttachPoseWristRotation.y + "," + AttachPoseWristRotation.z + "," + AttachPoseWristRotation.w;

        //     return packet;
        // }

        // public void AttachPoseFromString(string[] wristPos, string[] wristRot)
        // {
        //     AttachPoseWristPosition = new Vector3(float.Parse(wristPos[0]), float.Parse(wristPos[1]), float.Parse(wristPos[2]));
        //     AttachPoseWristRotation = new Quaternion(float.Parse(wristRot[0]), float.Parse(wristRot[1]), float.Parse(wristRot[2]), float.Parse(wristRot[3]));

        //     AttachPoseSet = true;
        // }

        private Quaternion RotationDeltaFromAttachWrist()
        {
            return DataMapper.WristJoint.rotation * Quaternion.Inverse(DataMapper.AttachPose.WristRotation) * RootSpawnRotation;
        }

        private void Awake()
        {
            DataMapper = FindObjectOfType<MrPuppetDataMapper>();
            HubConnection = FindObjectOfType<MrPuppetHubConnection>();

            RootSpawnPosition = Root.localPosition;
            RootSpawnRotation = Root.rotation;

            AttachAmountValue = 0;
            RotationModifiedTarget = Root.rotation;
            LerpTimer = 0;
        }

        private void Update()
        {
            if (DataMapper.AttachPoseSet)
            {
                // apply position delta to bind pose
                Vector3 position = RootSpawnPosition + ((DataMapper.WristJoint.position - (DataMapper.AttachPose.WristPosition)));

                RotationModifiedTarget = Quaternion.SlerpUnclamped(RootSpawnRotation, RotationDeltaFromAttachWrist(), RotationModifier);

                if(DeattachPoseSet)
                {
                    LerpTimer += Time.deltaTime;
                }

                if (LerpTimer > AttachAmountDuration) 
                    LerpTimer = AttachAmountDuration;
                AttachAmountValue = LerpTimer / AttachAmountDuration;

                RootRotationTarget = Quaternion.Slerp(RotationModifiedTarget, RootSpawnRotation, AttachAmountValue);
                Root.rotation = Quaternion.Slerp(Root.rotation, RootRotationTarget, RotationSpeed * Time.deltaTime);
                position = Vector3.Lerp(position, RootSpawnPosition, AttachAmountValue);

                // clamp to XYZ extents (BEFORE smooth)
                position.Set(
                    LimitRootExtentX ? Mathf.Clamp(position.x, RootSpawnPosition.x - RootExtentX, RootSpawnPosition.x + RootExtentX) : position.x,
                    LimitRootExtentY ? Mathf.Clamp(position.y, RootSpawnPosition.y - RootExtentY, RootSpawnPosition.y + RootExtentY) : position.y,
                    LimitRootExtentZ ? Mathf.Clamp(position.z, RootSpawnPosition.z - RootExtentZ, RootSpawnPosition.z + RootExtentZ) : position.z
                );

                // smoothly apply changes to position
                Root.localPosition = Vector3.SmoothDamp(Root.localPosition, position, ref PositionVelocity, PositionSpeed);

            }
        }

        private void OnDrawGizmos()
        {
            if (Root) Debug.DrawRay(Root.position, Root.up * 0.5f, Color.green, 0f, false);
            if (Root) Debug.DrawRay(Root.position, Root.right * 0.5f, Color.red, 0f, false);
            if (Root) Debug.DrawRay(Root.position, Root.forward * 0.5f, Color.blue, 0f, false);

            if (LimitRootExtentX)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(Application.isPlaying ? RootSpawnPosition : transform.position, new Vector3(RootExtentX * 2f, 0.001f, LimitRootExtentZ ? RootExtentZ * 2f : 0.1f));
            }

            if (LimitRootExtentY)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(Application.isPlaying ? RootSpawnPosition : transform.position, new Vector3(LimitRootExtentX ? RootExtentX * 2f : 0.1f, RootExtentY * 2f, 0.001f));
            }

            if (LimitRootExtentZ)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(Application.isPlaying ? RootSpawnPosition : transform.position, new Vector3(0.001f, LimitRootExtentY ? RootExtentY * 2f : 0.1f, RootExtentZ * 2f));
            }
        }
    }
}
