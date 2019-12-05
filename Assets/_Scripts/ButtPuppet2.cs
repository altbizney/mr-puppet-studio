using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPuppet.WIP
{
    public class ButtPuppet2 : MonoBehaviour
    {
        [ChildGameObjectsOnly]
        public Transform Hip;

        [ChildGameObjectsOnly]
        public Transform Head;

        private MrPuppetDataMapper DataMapper;

        private Quaternion SpawnHipRotation, SpawnHeadRotation;
        public Transform Raw, TPose, AttachPose, Delta, Final, Inverse;

        private void Awake()
        {
            if (!DataMapper) DataMapper = FindObjectOfType<MrPuppetDataMapper>();

            SpawnHipRotation = Final.localRotation;
        }

        private void LateUpdate()
        {
            Raw.localRotation = DataMapper.HubConnection.ElbowRotation;
            TPose.localRotation = DataMapper.TPose.ElbowRotation;
            AttachPose.localRotation = DataMapper.AttachPose.ElbowRotation;
            Delta.localRotation = DataMapper.GetJointRotationDelta(MrPuppetDataMapper.Joint.Elbow);
            Final.localRotation = SpawnHipRotation * DataMapper.GetJointRotationDelta(MrPuppetDataMapper.Joint.Elbow);
            Inverse.localRotation = Quaternion.Inverse(SpawnHipRotation) * DataMapper.GetJointRotationDelta(MrPuppetDataMapper.Joint.Elbow);

            // TODO: translate hip (optionally)
            // TODO: clamp hip translate
            // TODO: smooth hip translate

            // apply joint rotation delta onto initial rotation
            // Hip.localRotation = SpawnHipRotation * DataMapper.AttachPose.ElbowRotation;
            // Head.localRotation = SpawnHeadRotation * DataMapper.AttachPose.WristRotation;

            // Hip.rotation = (DataMapper.ElbowJoint.rotation * Quaternion.Inverse(AttachPoseElbowRotation)) * HipSpawnRotation);
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            Gizmos.color = Color.green;
            Gizmos.DrawRay(Raw.position, Raw.up * 0.25f);
            Gizmos.DrawRay(TPose.position, TPose.up * 0.25f);
            Gizmos.DrawRay(AttachPose.position, AttachPose.up * 0.25f);
            Gizmos.DrawRay(Delta.position, Delta.up * 0.25f);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(Raw.position, Raw.right * 0.25f);
            Gizmos.DrawRay(TPose.position, TPose.right * 0.25f);
            Gizmos.DrawRay(AttachPose.position, AttachPose.right * 0.25f);
            Gizmos.DrawRay(Delta.position, Delta.right * 0.25f);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(Raw.position, Raw.forward * 0.25f);
            Gizmos.DrawRay(TPose.position, TPose.forward * 0.25f);
            Gizmos.DrawRay(AttachPose.position, AttachPose.forward * 0.25f);
            Gizmos.DrawRay(Delta.position, Delta.forward * 0.25f);
        }
    }
}