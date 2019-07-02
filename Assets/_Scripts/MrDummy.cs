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
        public Transform JawTop;
        public Transform JawBottom;

        private void Update()
        {
            // Rotate the joints
            ShoulderJoint.localRotation = MrPuppet.FinalPose.ShoulderRotation;
            ElbowJoint.localRotation = MrPuppet.FinalPose.ElbowRotation;
            WristJoint.localRotation = MrPuppet.FinalPose.WristRotation;
        }
    }
}