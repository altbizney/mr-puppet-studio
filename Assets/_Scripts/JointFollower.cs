using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet
{
    public class JointFollower : MonoBehaviour
    {
        private MrPuppetDataMapper DataMapper;

        public MrPuppetDataMapper.Joint Joint = MrPuppetDataMapper.Joint.Wrist;

        public bool FollowPosition = true;
        public bool FollowRotation = true;

        [MinValue(0f)]
        public float RotationSpeed = 7f;
        [MinValue(0f)]
        public float PositionSpeed = 0.1f;

        public bool InLateUpdate = false;

        private Vector3 PositionVelocity;

        private void Awake()
        {
            DataMapper = FindObjectOfType<MrPuppetDataMapper>();
        }

        private void Update() { if (!InLateUpdate) Follow(); }
        private void LateUpdate() { if (InLateUpdate) Follow(); }

        private void Follow()
        {
            if (FollowRotation)
            {
                if (RotationSpeed > 0f)
                {
                    transform.localRotation = Quaternion.RotateTowards(transform.localRotation, DataMapper.GetJoint(Joint).rotation, RotationSpeed * Time.deltaTime);
                }
                else
                {
                    transform.localRotation = DataMapper.GetJoint(Joint).rotation;
                }
            }

            if (FollowPosition)
            {
                if (PositionSpeed > 0f)
                {
                    transform.localPosition = Vector3.SmoothDamp(transform.localPosition, DataMapper.GetJoint(Joint).position, ref PositionVelocity, PositionSpeed);
                }
                else
                {
                    transform.localPosition = DataMapper.GetJoint(Joint).position;
                }
            }
        }
    }
}