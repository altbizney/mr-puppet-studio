using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet
{
    public class JointFollower : MonoBehaviour
    {
        [Required]
        public MrPuppetDataMapper DataMapper;

        public MrPuppetDataMapper.Joint Joint = MrPuppetDataMapper.Joint.Wrist;

        [MinValue(0f)]
        public float RotationSpeed = 7f;
        [MinValue(0f)]
        public float PositionSpeed = 0.1f;

        private Vector3 PositionVelocity;

        private void OnValidate()
        {
            if (!DataMapper) DataMapper = FindObjectOfType<MrPuppetDataMapper>();
        }

        private void Update()
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, DataMapper.GetJoint(Joint).rotation, RotationSpeed * Time.smoothDeltaTime);
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, DataMapper.GetJoint(Joint).position, ref PositionVelocity, PositionSpeed);
        }
    }
}