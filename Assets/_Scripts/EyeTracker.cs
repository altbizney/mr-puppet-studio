using UnityEngine;
using UnityEditor;

namespace MrPuppet
{
    public class EyeTracker : MonoBehaviour
    {
        public Transform GazeTarget;

        public Transform Eyeball;
        public Transform Pupil;

        public MeshCollider Collider;
        public MeshCollider ConvexCollider;

        private Vector3 point;
        private Ray ray;
        private RaycastHit hit;
        private bool collided;

        private void LateUpdate()
        {
            if (!GazeTarget) return;

            // find closest point on eyeball collider to driver
            point = ConvexCollider.ClosestPoint(GazeTarget.position);

            // find the direction from gazetarget to point on eyeball
            var heading = point - GazeTarget.position;
            var distance = heading.magnitude;
            var direction = heading / distance;

            ray = new Ray(GazeTarget.position, direction + heading);
            collided = Collider.Raycast(ray, out hit, Mathf.Infinity);
        }

        private void Update()
        {
            // move pupil to where the ray hit the eyeball
            if (collided)
            {
                Pupil.SetPositionAndRotation(hit.point, Quaternion.FromToRotation(Vector3.forward, GetMeshColliderNormal(hit)));
            }
            else
            {
                Pupil.position = point;
            }
        }

        private void OnDrawGizmos()
        {
            if (!GazeTarget) return;

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(GazeTarget.position, 0.05f);
            Gizmos.DrawSphere(point, 0.05f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hit.point, 0.05f);
            Gizmos.DrawRay(ray);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(hit.point, hit.normal);

            Gizmos.color = Color.yellow;
            // Debug.Log(GetMeshColliderNormal(hit));
            Gizmos.DrawRay(hit.point, GetMeshColliderNormal(hit));
        }

        // https://answers.unity.com/questions/50846/how-do-i-obtain-the-surface-normal-for-a-point-on.html
        private Vector3 GetMeshColliderNormal(RaycastHit hit)
        {
            if (hit.collider == null || hit.triangleIndex < 0) return Vector3.zero;

            MeshCollider collider = (MeshCollider)hit.collider;
            Mesh mesh = collider.sharedMesh;
            Vector3[] normals = mesh.normals;
            int[] triangles = mesh.triangles;

            Vector3 n0 = normals[triangles[hit.triangleIndex * 3 + 0]];
            Vector3 n1 = normals[triangles[hit.triangleIndex * 3 + 1]];
            Vector3 n2 = normals[triangles[hit.triangleIndex * 3 + 2]];
            Vector3 baryCenter = hit.barycentricCoordinate;
            Vector3 interpolatedNormal = n0 * baryCenter.x + n1 * baryCenter.y + n2 * baryCenter.z;
            interpolatedNormal.Normalize();
            interpolatedNormal = hit.transform.TransformDirection(interpolatedNormal);

            return interpolatedNormal;
        }
    }
}
