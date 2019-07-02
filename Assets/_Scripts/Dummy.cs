using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Thinko
{
    public class Dummy : MonoBehaviour
    {
        [Required]
        public RealPuppetDataProvider DataProvider;

        public Transform ShoulderJoint;
        public Transform ElbowJoint;
        public Transform WristJoint;
        public Transform JawTop;
        public Transform JawBottom;

        private void Update()
        {
            // Rotate the joints
            ShoulderJoint.localRotation = DataProvider.GetInput(RealPuppetDataProvider.Source.Shoulder);
            ElbowJoint.localRotation = DataProvider.GetInput(RealPuppetDataProvider.Source.Elbow);
            WristJoint.rotation = DataProvider.GetInput(RealPuppetDataProvider.Source.Wrist);

            // TODO: apply tpose
        }
    }
}