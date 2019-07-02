using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Thinko
{
    public class MrDummy : MonoBehaviour
    {
        [Required]
        public RealBody MrPuppet;

        public Transform ShoulderJoint;
        public Transform ElbowJoint;
        public Transform WristJoint;

        public Transform JawJoint;

        private void Update()
        {
            // Rotate the joints
            ShoulderJoint.localRotation = MrPuppet.FinalPose.ShoulderRotation;
            ElbowJoint.localRotation = MrPuppet.FinalPose.ElbowRotation;
            WristJoint.localRotation = MrPuppet.FinalPose.WristRotation;

            JawJoint.localEulerAngles = Vector3.forward * Mathf.LerpAngle(0f, -45.0f, 0.5f);
        }
    }
}