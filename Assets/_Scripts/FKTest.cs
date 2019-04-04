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
            // WristNode.localRotation = Quaternion.Euler(RealPuppetDataProvider.Rotation.eulerAngles - RealPuppetDataProvider.Rotation2.eulerAngles);
            WristNode.localRotation = RealPuppetDataProvider.Rotation * Quaternion.Inverse(RealPuppetDataProvider.Rotation2);
        }
    }
}