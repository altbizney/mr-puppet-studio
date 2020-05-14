﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet
{
    public class ButtPuppet : MonoBehaviour
    {
        [Serializable]
        public class WeightedInfluence
        {
            public MrPuppetDataMapper.Joint joint;
            public Transform target;
            // private Transform proxy;

            [Range(0f, 1f)]
            public float amount = 1f;

            private Quaternion spawn;
            private Quaternion full;
            private Quaternion weighted;
            private Quaternion attach;


            public void SnapshotSpawn()
            {
                // proxy = new GameObject("Proxy:" + target.name).transform;
                // proxy.SetPositionAndRotation(target.position, target.rotation);
                // proxy.SetParent(target.parent, false);

                spawn = target.rotation;
            }

            private Quaternion Attach(MrPuppetDataMapper DataMapper)
            {
                switch (joint)
                {
                    case MrPuppetDataMapper.Joint.Elbow:
                        return DataMapper.AttachPose.ElbowRotation;
                    case MrPuppetDataMapper.Joint.Wrist:
                        return DataMapper.AttachPose.WristRotation;
                    default:
                        return Quaternion.identity;
                }
            }

            public void Update(MrPuppetDataMapper DataMapper, float RotationSpeed, float AttachAmount)
            {
                if (!target) return;

                // calculate fully blended extent
                full = (DataMapper.GetJoint(joint).rotation * Quaternion.Inverse(Attach(DataMapper))) * spawn;

                // calculate weighted rotation
                weighted = Quaternion.Slerp(spawn, full, amount * AttachAmount);
                
                // calculate deattach rotation
                //attach = Quaternion.Slerp(weighted, spawn, AttachAmount);

                // apply with smoothing
                target.rotation = Quaternion.Slerp(target.rotation, attach, RotationSpeed * Time.deltaTime);
            }

            public void OnDrawGizmos()
            {
                if (!target) return;

                Debug.DrawRay(target.position, target.up * 0.5f, Color.green, 0f, false);
                Debug.DrawRay(target.position, target.right * 0.5f, Color.red, 0f, false);
                Debug.DrawRay(target.position, target.forward * 0.5f, Color.blue, 0f, false);
            }
        }

        private MrPuppetDataMapper DataMapper;
        private MrPuppetHubConnection HubConnection;

        // spawn position of proxy geo
        private Vector3 HipSpawnPosition;
        private Quaternion HipSpawnRotation;
        private Quaternion HeadSpawnRotation;

        public Transform Hip;
        public Transform Head;
        // private Transform HipProxy;
        // private Transform HeadProxy;

        private Quaternion UnsubscribeHipRotation;
        private Quaternion UnsubscribeHeadRotation;
        private bool Unsubscribed;
        private bool UnsubscribeForward;
        private float SensorAmount;
        private float LerpTimer;

        private Vector3 position;

        public List<WeightedInfluence> WeightedInfluences = new List<WeightedInfluence>();

        [MinValue(0f)]
        public float RotationSpeed = 7f;
        [MinValue(0f)]
        public float PositionSpeed = 0.1f;
        private Vector3 PositionVelocity;

        [HorizontalGroup("HipExtentX")]
        public bool LimitHipExtentX = false;
        [HorizontalGroup("HipExtentX")]
        [ShowIf("LimitHipExtentX")]
        public float HipExtentX = 0f;

        [HorizontalGroup("HipExtentY")]
        public bool LimitHipExtentY = false;
        [HorizontalGroup("HipExtentY")]
        [ShowIf("LimitHipExtentY")]
        public float HipExtentY = 0f;

        [HorizontalGroup("HipExtentZ")]
        public bool LimitHipExtentZ = false;
        [HorizontalGroup("HipExtentZ")]
        [ShowIf("LimitHipExtentZ")]
        public float HipExtentZ = 0f;

        public bool EnableJawHeadMixer = false;
        [ShowIf("EnableJawHeadMixer")]
        public float JawHeadMaxExtent = 10f;

        public enum JawHeadAxis { x, y, z }

        [ShowIf("EnableJawHeadMixer")]
        [EnumToggleButtons]
        public JawHeadAxis JawHeadRotate = JawHeadAxis.z;

        [HideInInspector]
        public bool ApplySensors = true;

        [MinValue(0.01f)]
        public float UnsubscribeDuration = 3f;

        [Button(ButtonSizes.Large)]
        [GUIColor(0f, 1f, 0f)]
        [DisableInEditorMode()]
        public void UnsubscribeFromSensors()
        {
            if(!Unsubscribed)
            {
                LerpTimer = 0;
                Unsubscribed = true;
                SensorAmount = 0;
                UnsubscribeForward = true;
            }
        }

        public void SubscribeEventButtPuppet()
        {
           if(Unsubscribed)
           {
                UnsubscribeForward = false;
                LerpTimer = UnsubscribeDuration;
           }
        }

        private void Awake()
        {
            DataMapper = FindObjectOfType<MrPuppetDataMapper>();
            HubConnection = FindObjectOfType<MrPuppetHubConnection>();

            ApplySensors = true;

            // // clone proxy geo
            // HipProxy = new GameObject("Proxy:" + Hip.name).transform;
            // HipProxy.SetPositionAndRotation(Hip.position, Hip.rotation);
            // HipProxy.SetParent(Hip.parent, false);

            // HeadProxy = new GameObject("Proxy:" + Head.name).transform;
            // HeadProxy.SetPositionAndRotation(Head.position, Head.rotation);
            // HeadProxy.SetParent(Head.parent, false);

            HipSpawnPosition = Hip.localPosition;
            HipSpawnRotation = Hip.rotation;
            HeadSpawnRotation = Head.rotation;

            // snapshot bind poses of weighted influence targets
            foreach (var influence in WeightedInfluences)
            {
                influence.SnapshotSpawn();
            }

            DataMapper.OnSubscribeEvent += SubscribeEventButtPuppet;
        }

        private void Update()
        {
            if (DataMapper.AttachPoseSet)
            {
                // apply position delta to bind pose
                if (ApplySensors == true)
                {
                    position = HipSpawnPosition + (DataMapper.ElbowAnchorJoint.position - DataMapper.AttachPose.ElbowPosition);

                    if(Unsubscribed && UnsubscribeForward)
                    {
                        LerpTimer += Time.deltaTime;
                    }
                    else if(Unsubscribed && !UnsubscribeForward)
                    {
                        LerpTimer -= Time.deltaTime;
                    }

                    if (LerpTimer > UnsubscribeDuration && UnsubscribeForward)
                    { 
                        LerpTimer = UnsubscribeDuration;
                    }
                    else if(LerpTimer < 0 && !UnsubscribeForward)
                    {
                        Unsubscribed = false;
                        LerpTimer = 0;
                        SensorAmount = 0;
                    }

                    SensorAmount = LerpTimer / UnsubscribeDuration;

                    position = Vector3.Lerp(position, HipSpawnPosition, SensorAmount);
                    UnsubscribeHipRotation = Quaternion.Slerp((DataMapper.ElbowJoint.rotation * Quaternion.Inverse(DataMapper.AttachPose.ElbowRotation)) * HipSpawnRotation, HipSpawnRotation, SensorAmount);
                    UnsubscribeHeadRotation = Quaternion.Slerp((DataMapper.WristJoint.rotation * Quaternion.Inverse(DataMapper.AttachPose.WristRotation)) * HeadSpawnRotation, HeadSpawnRotation, SensorAmount);

                    // clamp to XYZ extents (BEFORE smooth)
                    position.Set(
                        LimitHipExtentX ? Mathf.Clamp(position.x, HipSpawnPosition.x - HipExtentX, HipSpawnPosition.x + HipExtentX) : position.x,
                        LimitHipExtentY ? Mathf.Clamp(position.y, HipSpawnPosition.y - HipExtentY, HipSpawnPosition.y + HipExtentY) : position.y,
                        LimitHipExtentZ ? Mathf.Clamp(position.z, HipSpawnPosition.z - HipExtentZ, HipSpawnPosition.z + HipExtentZ) : position.z
                    );

                    // smoothly apply changes to position
                    Hip.localPosition = Vector3.SmoothDamp(Hip.localPosition, position, ref PositionVelocity, PositionSpeed);

                    // apply rotation deltas to bind pose
                    Hip.rotation = Quaternion.Slerp(Hip.rotation, UnsubscribeHipRotation, RotationSpeed * Time.deltaTime);
                    Head.rotation = Quaternion.Slerp(Head.rotation, UnsubscribeHeadRotation, RotationSpeed * Time.deltaTime);

                    if (EnableJawHeadMixer)
                    {
                        switch (JawHeadRotate)
                        {
                            case JawHeadAxis.x:
                                Head.Rotate(Mathf.Lerp(0f, JawHeadMaxExtent, DataMapper.JawPercent), 0f, 0f, Space.Self);
                                break;

                            case JawHeadAxis.y:
                                Head.Rotate(0f, Mathf.Lerp(0f, JawHeadMaxExtent, DataMapper.JawPercent), 0f, Space.Self);
                                break;

                            case JawHeadAxis.z:
                                Head.Rotate(0f, 0f, Mathf.Lerp(0f, JawHeadMaxExtent, DataMapper.JawPercent), Space.Self);
                                break;
                        }
                    }

                    // apply weighted influences
                    foreach (var influence in WeightedInfluences)
                    {
                        influence.Update(DataMapper, RotationSpeed, SensorAmount);
                    }

                    if (Input.GetKeyDown(KeyCode.D)) { UnsubscribeFromSensors(); }
                }
            }
        }

        // REMINDER: Change = Quaternion.Inverse(Last) * Current;

        // private void LateUpdate()
        // {
        //     if (!(HipProxy && HeadProxy)) return;

        //     Hip.position = Hip.position - (HipProxySpawnPosition - HipProxy.position);

        //     Hip.rotation = Hip.rotation * (Quaternion.Inverse(Hip.rotation) * HipProxy.rotation);
        //     Head.rotation = Head.rotation * (Quaternion.Inverse(Head.rotation) * HeadProxy.rotation);

        //     // WIP: this basically is world-relative. also sort of busted on head....
        //     // Hip.rotation = Hip.rotation * (HipProxy.rotation * Quaternion.Inverse(HipProxySpawnRotation));
        //     // Head.rotation = Head.rotation * (HeadProxy.localRotation * Quaternion.Inverse(HeadProxySpawnRotation));
        // }

        private void OnDrawGizmos()
        {
            if (Hip) Debug.DrawRay(Hip.position, Hip.up * 0.5f, Color.green, 0f, false);
            if (Head) Debug.DrawRay(Head.position, Head.up * 0.5f, Color.green, 0f, false);

            if (Hip) Debug.DrawRay(Hip.position, Hip.right * 0.5f, Color.red, 0f, false);
            if (Head) Debug.DrawRay(Head.position, Head.right * 0.5f, Color.red, 0f, false);

            if (Hip) Debug.DrawRay(Hip.position, Hip.forward * 0.5f, Color.blue, 0f, false);
            if (Head) Debug.DrawRay(Head.position, Head.forward * 0.5f, Color.blue, 0f, false);

            foreach (var influence in WeightedInfluences) influence.OnDrawGizmos();

            if (LimitHipExtentX)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(Application.isPlaying ? HipSpawnPosition : transform.position, new Vector3(HipExtentX * 2f, 0.001f, LimitHipExtentZ ? HipExtentZ * 2f : 0.1f));
            }

            if (LimitHipExtentY)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(Application.isPlaying ? HipSpawnPosition : transform.position, new Vector3(LimitHipExtentX ? HipExtentX * 2f : 0.1f, HipExtentY * 2f, 0.001f));
            }

            if (LimitHipExtentZ)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(Application.isPlaying ? HipSpawnPosition : transform.position, new Vector3(0.001f, LimitHipExtentY ? HipExtentY * 2f : 0.1f, HipExtentZ * 2f));
            }
        }
    }
}
