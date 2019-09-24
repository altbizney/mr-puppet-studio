using UnityEngine;
using UnityEditor;

namespace MrPuppet
{
    public class EyeTracker : MonoBehaviour
    {
        public Transform GazeTarget;

        public Transform Eyeball;
        public Transform Pupil;

        public MeshCollider EyeballCollider;

        private Vector3 point;

        private Ray ray;
        private RaycastHit hit;

        private void LateUpdate()
        {
            if (!GazeTarget) return;

            // find closest point on eyeball collider to driver
            point = EyeballCollider.ClosestPoint(GazeTarget.position);

            // find the direction, and extend that point a bit (so we're INSIDE the eyeball)
            Vector3 direction = point - GazeTarget.position;
            point += direction.normalized * 0.01f;

            // cast a ray from gaze target in the direction we just computed
            ray = new Ray(GazeTarget.position, direction);
            EyeballCollider.Raycast(ray, out hit, Mathf.Infinity);
        }

        private void Update()
        {
            // move pupil to where the ray hit the eyeball
            Pupil.SetPositionAndRotation(hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal));
        }

        private void OnDrawGizmos()
        {
            if (!GazeTarget) return;

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(GazeTarget.position, 0.05f);
            Gizmos.DrawSphere(point, 0.05f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hit.point, 0.05f);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(hit.point, hit.normal);
        }
    }
}
