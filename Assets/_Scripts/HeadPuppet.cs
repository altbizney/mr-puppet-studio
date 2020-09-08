using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using Sirenix.OdinInspector;

namespace MrPuppet
{
    public class HeadPuppet : MonoBehaviour
    {
        private MrPuppetDataMapper DataMapper;
        private MrPuppetHubConnection HubConnection;

        private Vector3 RootSpawnPosition;
        private Quaternion RootSpawnRotation;

        public Transform Root;
        private Vector3 position;

        private Quaternion RootRotationTarget;
        private Quaternion RotationModifiedTarget;
        private bool Unsubscribed = true;
        private bool UnsubscribeForward;
        private string UnsubscribeButtonLabel = "Hardware control disabled. Attach to enable";
        private float LerpTimer;
        private List<JawBlendShapeMapper> JawBlendshapeComponents = new List<JawBlendShapeMapper>();
        private List<JawTransformMapper> JawTransformComponents = new List<JawTransformMapper>();

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

        [MinValue(0.01f)]
        [TitleGroup("Sensor Subscription")]
        [OnValueChanged("ChangedDuration")]
        public float UnsubscribeDuration = 1f;

        [ReadOnly]
        [Range(0f, 1f)]
        [TitleGroup("Sensor Subscription")]
        public float SensorAmount = 0f;

        [Button(ButtonSizes.Large)]
        [GUIColor(0f, 1f, 0f)]
        [DisableInEditorMode()]
        [TitleGroup("Sensor Subscription")]
        [LabelText("$UnsubscribeButtonLabel")]
        [DisableIf("$Unsubscribed")]
        public void UnsubscribeFromSensors()
        {
            if (!Unsubscribed)
            {
                LerpTimer = UnsubscribeDuration;
                Unsubscribed = true;
                SensorAmount = 0;
                UnsubscribeForward = false;
                UnsubscribeButtonLabel = "Hardware control disabled. Re-attach to enable";
            }
        }

        private void ChangedDuration()
        {
            if (!Unsubscribed)
            {
                LerpTimer = UnsubscribeDuration;
                SensorAmount = 1f;
            }
        }

        public void SubscribeEventHeadPuppet()
        {
            if (Unsubscribed)
            {
                UnsubscribeForward = true;
            }
        }

        //deattach only deattaches.
        //remove ability to ease back from deattach button
        //put it in attach eventheadpuppet

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

            SensorAmount = 0;
            RotationModifiedTarget = Root.rotation;
            LerpTimer = 0;

            DataMapper.OnSubscribeEvent += SubscribeEventHeadPuppet;
            foreach (JawTransformMapper jaw in gameObject.GetComponentsInChildren<JawTransformMapper>())
            {
                JawTransformComponents.Add(jaw);
            }
            foreach (JawBlendShapeMapper jaw in gameObject.GetComponentsInChildren<JawBlendShapeMapper>())
            {
                JawBlendshapeComponents.Add(jaw);
            }
        }

        private void Update()
        {
            if (DataMapper.AttachPoseSet)
            {
                // apply position delta to bind pose
                position = RootSpawnPosition + ((DataMapper.WristJoint.position - (DataMapper.AttachPose.WristPosition)));

                RotationModifiedTarget = Quaternion.SlerpUnclamped(RootSpawnRotation, RotationDeltaFromAttachWrist(), RotationModifier);

                if (Unsubscribed)
                {
                    if (UnsubscribeForward)
                        LerpTimer += Time.deltaTime;
                    else
                        LerpTimer -= Time.deltaTime;

                    SensorAmount = LerpTimer / UnsubscribeDuration;
                    SensorAmount = SensorAmount * SensorAmount * (3f - 2f * SensorAmount);
                }

                if (LerpTimer > UnsubscribeDuration && UnsubscribeForward)
                {
                    LerpTimer = UnsubscribeDuration;
                    UnsubscribeButtonLabel = "Disable hardware control";
                    Unsubscribed = false;
                    SensorAmount = 1;
                }
                else if (LerpTimer < 0 && !UnsubscribeForward)
                {
                    LerpTimer = 0;
                    SensorAmount = 0;
                }

                RootRotationTarget = Quaternion.Slerp(RootSpawnRotation, RotationModifiedTarget, SensorAmount);
                Root.rotation = Quaternion.Slerp(Root.rotation, RootRotationTarget, RotationSpeed * Time.deltaTime);
                position = Vector3.Lerp(RootSpawnPosition, position, SensorAmount);

                foreach (JawBlendShapeMapper Jaw in JawBlendshapeComponents) { Jaw.SensorAmount = SensorAmount; }
                foreach (JawTransformMapper Jaw in JawTransformComponents) { Jaw.SensorAmount = SensorAmount; }

                // clamp to XYZ extents (BEFORE smooth)
                position.Set(
                    LimitRootExtentX ? Mathf.Clamp(position.x, RootSpawnPosition.x - RootExtentX, RootSpawnPosition.x + RootExtentX) : position.x,
                    LimitRootExtentY ? Mathf.Clamp(position.y, RootSpawnPosition.y - RootExtentY, RootSpawnPosition.y + RootExtentY) : position.y,
                    LimitRootExtentZ ? Mathf.Clamp(position.z, RootSpawnPosition.z - RootExtentZ, RootSpawnPosition.z + RootExtentZ) : position.z
                );

                // smoothly apply changes to position
                Root.localPosition = Vector3.SmoothDamp(Root.localPosition, position, ref PositionVelocity, PositionSpeed);

                if (Input.GetKeyDown(KeyCode.D)) { UnsubscribeFromSensors(); }
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
