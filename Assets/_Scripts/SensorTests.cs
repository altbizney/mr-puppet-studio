using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet
{
    public class SensorTests : MonoBehaviour
    {
        [Required]
        public MrPuppetHubConnection HubConnection;

        private void OnValidate()
        {
            if (HubConnection == null) HubConnection = FindObjectOfType<MrPuppetHubConnection>();
        }

        private void Update()
        {
            transform.rotation = HubConnection.WristRotation;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(Vector3.zero, transform.forward);

            Gizmos.color = Color.green;
            Gizmos.DrawRay(Vector3.zero, transform.up);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(Vector3.zero, transform.right);
        }
    }
}