using UnityEngine;

namespace Thinko
{
    public class ProxyTransformDataProvider : RealPuppetDataProvider
    {
        public Transform Wrist;
        public Transform Elbow;
        public Transform Shoulder;

        private void LateUpdate()
        {
            if(Wrist != null)
                WristRotation = Wrist.rotation;
            
            if(Elbow != null)
                ElbowRotation = Elbow.rotation;
            
            if(Shoulder != null)
                ShoulderRotation = Shoulder.rotation;
        }
    }
}