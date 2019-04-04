using UnityEngine;

namespace Thinko
{
    public class FKTest : MonoBehaviour
    {
        public RealPuppetDataProvider RealPuppetDataProvider;
        
        [Header("Joints")]
        public Transform ElbowNode;
        public Transform WristNode;
        
        private void Update()
        {
            if (RealPuppetDataProvider == null)
            {
                Debug.LogWarning("No RealPuppetDataProvider available");
                return;
            }
            
            ElbowNode.localRotation = RealPuppetDataProvider.Rotation2;            
            WristNode.localRotation = RealPuppetDataProvider.Rotation;
        }
    }
}