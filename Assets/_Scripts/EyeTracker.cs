using UnityEngine;
using UnityEditor;

namespace MrPuppet
{
    public class EyeTracker : MonoBehaviour
    {
        public Transform PupilDriver;

        public Transform Eyeball;
        public Transform Pupil;

        public Collider EyeballCollider;

        private Ray ray;
        private RaycastHit hit;

        private void FixedUpdate()
        {
            if (!PupilDriver) return;

            // cast ray from driver backward (towards eye). TODO: find that direction dynamically
            ray = new Ray(PupilDriver.position, -PupilDriver.forward);

            // cast the ray exclusively to the eyeball collider
            EyeballCollider.Raycast(ray, out hit, 10f);
        }

        private void Update()
        {
            // Debug.DrawRay(Eyeball.position, Eyeball.up, Color.green);

            // move pupil to where the ray hit the eyeball
            // TODO: keep rotation relative to "up" of Eyeball
            Pupil.SetPositionAndRotation(hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal));
        }

        private void OnDrawGizmos()
        {
            if (!PupilDriver) return;

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(PupilDriver.position, 0.05f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hit.point, 0.05f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(ray.origin, ray.direction);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(hit.point, hit.normal);
        }
    }
}
