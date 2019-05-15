using UnityEngine;

namespace Thinko
{
    public class FKTest : MonoBehaviour
    {
        public RealPuppetDataProvider RealPuppetDataProvider;

        [Header("Joints")]
        public Transform WristNode;
        public Transform ElbowNode;
        public Transform ShoulderNode;

        private void Update()
        {
            if (RealPuppetDataProvider == null)
            {
                Debug.LogWarning("No RealPuppetDataProvider available");
                return;
            }

            // subtract parent quaternions down the chain
            ShoulderNode.rotation = RealPuppetDataProvider.ShoulderRotation;
            ElbowNode.rotation = RealPuppetDataProvider.ElbowRotation;// * Quaternion.Inverse(RealPuppetDataProvider.ShoulderRotation);
            WristNode.rotation = RealPuppetDataProvider.WristRotation;// * Quaternion.Inverse(RealPuppetDataProvider.ElbowRotation);
        }

        private void OnDrawGizmos()
        {
            if (RealPuppetDataProvider != null)
            {
                Debug.DrawRay(WristNode.position, RealPuppetDataProvider.WristRotation * transform.forward, Color.blue, 0f, true);
                Debug.DrawRay(WristNode.position, RealPuppetDataProvider.WristRotation * transform.up, Color.green, 0f, true);
                Debug.DrawRay(WristNode.position, RealPuppetDataProvider.WristRotation * transform.right, Color.red, 0f, true);

                Debug.DrawRay(ElbowNode.position, RealPuppetDataProvider.ElbowRotation * transform.forward, Color.blue, 0f, true);
                Debug.DrawRay(ElbowNode.position, RealPuppetDataProvider.ElbowRotation * transform.up, Color.green, 0f, true);
                Debug.DrawRay(ElbowNode.position, RealPuppetDataProvider.ElbowRotation * transform.right, Color.red, 0f, true);

                Debug.DrawRay(ShoulderNode.position, RealPuppetDataProvider.ShoulderRotation * transform.forward, Color.blue, 0f, true);
                Debug.DrawRay(ShoulderNode.position, RealPuppetDataProvider.ShoulderRotation * transform.up, Color.green, 0f, true);
                Debug.DrawRay(ShoulderNode.position, RealPuppetDataProvider.ShoulderRotation * transform.right, Color.red, 0f, true);
            }
        }
    }
}