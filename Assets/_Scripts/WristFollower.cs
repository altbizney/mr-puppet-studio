using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet
{
    public class WristFollower : MonoBehaviour
    {
        [Required]
        public MrPuppetDataMapper DataMapper;

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
            // TODO: convert to JointFollower and make this dynamic
            transform.localRotation = Quaternion.Slerp(transform.localRotation, DataMapper.WristJoint.rotation, RotationSpeed * Time.deltaTime);
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, DataMapper.WristJoint.position, ref PositionVelocity, PositionSpeed);
        }
    }
}