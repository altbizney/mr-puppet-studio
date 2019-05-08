using System.Collections.Generic;
using UnityEngine;

namespace Thinko
{
    public class RealBody : MonoBehaviour
    {
        public bool SetLocalRotation;
        public List<RealPuppet.PuppetJoint> PuppetJoints = new List<RealPuppet.PuppetJoint>();

        private void Update()
        {
            foreach (var puppetJoint in PuppetJoints)
            {
                if (!puppetJoint.Enabled || puppetJoint.RealPuppetDataProvider == null || puppetJoint.Joint == null) continue;

                if (SetLocalRotation)
                {
                    puppetJoint.Joint.localRotation = Quaternion.Slerp(
                        puppetJoint.Joint.localRotation, 
                        Quaternion.Inverse(puppetJoint.Joint.parent.localRotation) * puppetJoint.RealPuppetDataProvider.GetInput(puppetJoint.InputSource) * Quaternion.Euler(puppetJoint.Offset), 
                        puppetJoint.Sharpness);
                }
                else
                {
                    puppetJoint.Joint.rotation = Quaternion.Slerp(
                        puppetJoint.Joint.rotation, 
                        puppetJoint.RealPuppetDataProvider.GetInput(puppetJoint.InputSource) * Quaternion.Euler(puppetJoint.Offset),// * Quaternion.Inverse(puppetJoint.TPose), 
                        puppetJoint.Sharpness);
                }
            }
        }
    }
}