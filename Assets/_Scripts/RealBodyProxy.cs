using Sirenix.OdinInspector;
using UnityEngine;

namespace Thinko
{
    public class RealBodyProxy : MonoBehaviour
    {
        [Required]
        public RealBody RealBody;

        public Transform ShoulderJoint;
        public Transform ElbowJoint;
        public Transform WristJoint;

        private bool _attached;

        [Button(ButtonSizes.Large)]
        [HideIf("_attached")]
        [GUIColor(0f, 1f, 0f)]
        public void Attach()
        {
            _attached = true;
        }
        
        [Button(ButtonSizes.Large)]
        [ShowIf("_attached")]
        [GUIColor(1f, 0f, 0f)]
        public void Detach()
        {
            _attached = false;
        }

        private void Update()
        {
            if (!_attached) return;
            
            if (ShoulderJoint)
                ShoulderJoint.rotation = RealBody.FinalPose.ShoulderRotation;
                
            if (ElbowJoint)
                ElbowJoint.rotation = RealBody.FinalPose.ElbowRotation;
                
            if (WristJoint)
                WristJoint.rotation = RealBody.FinalPose.WristRotation;
        }
    }
}