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

        private Quaternion WristTPose = Quaternion.identity;
        private Quaternion ElbowTPose = Quaternion.identity;
        private Quaternion ShoulderTPose = Quaternion.identity;

        private void Update()
        {
            if (RealPuppetDataProvider == null)
            {
                Debug.LogWarning("No RealPuppetDataProvider available");
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                ShoulderTPose = RealPuppetDataProvider.ShoulderRotation;
                ElbowTPose = RealPuppetDataProvider.ElbowRotation;
                WristTPose = RealPuppetDataProvider.WristRotation;
            }

            ShoulderNode.rotation = RealPuppetDataProvider.ShoulderRotation * Quaternion.Inverse(ShoulderTPose);
            ElbowNode.rotation = RealPuppetDataProvider.ElbowRotation * Quaternion.Inverse(ElbowTPose);
            WristNode.rotation = RealPuppetDataProvider.WristRotation * Quaternion.Inverse(WristTPose);
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