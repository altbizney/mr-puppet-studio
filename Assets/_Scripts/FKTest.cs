using UnityEngine;

namespace Thinko
{
    public class FKTest : MonoBehaviour
    {
        public RealPuppetDataProvider RealPuppetDataProvider;
        
        [Header("Joints")]
        public Transform WristNode;
        public Transform ElbowNode;
        
        private void Update()
        {
            if (RealPuppetDataProvider == null)
            {
                Debug.LogWarning("No RealPuppetDataProvider available");
                return;
            }
            
            WristNode.localRotation = RealPuppetDataProvider.Rotation;
            ElbowNode.localRotation = RealPuppetDataProvider.Rotation2;            
        }
    }
}