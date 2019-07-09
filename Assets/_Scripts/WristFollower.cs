using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet
{
    public class WristFollower : MonoBehaviour
    {
        [Required]
        public MrPuppetDataMapper DataMapper;

        private void OnValidate()
        {
            if (!DataMapper) DataMapper = FindObjectOfType<MrPuppetDataMapper>();
        }

        private void Update()
        {
            // TODO: convert to JointFollower and make this dynamic
            transform.localPosition = DataMapper.WristJoint.position;
            transform.localRotation = DataMapper.WristJoint.rotation;
        }
    }
}