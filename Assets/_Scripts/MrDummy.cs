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
            ShoulderJoint.rotation = MrPuppet.FinalPose.ShoulderRotation;
            ElbowJoint.rotation = MrPuppet.FinalPose.ElbowRotation;
            WristJoint.rotation = MrPuppet.FinalPose.WristRotation;

            JawJoint.localEulerAngles = Vector3.forward * Mathf.LerpAngle(0f, -45.0f, MrPuppet.JawPercent);
        }
    }
}