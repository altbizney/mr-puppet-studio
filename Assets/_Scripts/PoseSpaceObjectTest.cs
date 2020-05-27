using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


namespace MrPuppet
{
    public class PoseSpaceObjectTest : MonoBehaviour
    {

        Quaternion Snapshot;
        Quaternion DeltaQuat;

        [ReadOnly]
        public Vector3 DeltaEuler;
        [ReadOnly]
        public Vector3 DeltaClamped;
        [ReadOnly]
        public Vector3 DeltaAngleMethod;

        void Awake()
        {
            SnapshotRotation();
            //transform.rotation *= Quaternion.AngleAxis(30, Vector3.up);
            //transform.rotation *= Quaternion.AngleAxis(30, Vector3.down);
        }

        void Update()
        {
            DeltaQuat = gameObject.transform.localRotation * Quaternion.Inverse(Snapshot);

            DeltaAngleMethod.y = Mathf.DeltaAngle(Snapshot.eulerAngles.y, gameObject.transform.localRotation.eulerAngles.y);
            DeltaAngleMethod.x = Mathf.DeltaAngle(Snapshot.eulerAngles.x, gameObject.transform.localRotation.eulerAngles.x);
            DeltaAngleMethod.z = Mathf.DeltaAngle(Snapshot.eulerAngles.z, gameObject.transform.localRotation.eulerAngles.z);

            DeltaClamped = EulerAnglesClamp(DeltaQuat);
            DeltaEuler = DeltaQuat.eulerAngles;

            //DeltaSeperate.y = SeperateAxisY(DeltaQuat).eulerAngles.y;
            //DeltaSeperate.x = SeperateAxisX(DeltaQuat).eulerAngles.x;
            //DeltaSeperate.z = SeperateAxisZ(DeltaQuat).eulerAngles.z;
        }

        [Button(ButtonSizes.Large)]
        public void SnapshotRotation()
        {
            Snapshot = gameObject.transform.localRotation;
        }

        [Button(ButtonSizes.Large)]
        public void RotateUp()
        {
            transform.rotation *= Quaternion.AngleAxis(10, Vector3.up);
        }


        [Button(ButtonSizes.Large)]
        public void RotateDown()
        {
            transform.rotation *= Quaternion.AngleAxis(10, Vector3.down);
        }


        [Button(ButtonSizes.Large)]
        public void RotateLeft()
        {
            transform.rotation *= Quaternion.AngleAxis(10, Vector3.left);
        }

        [Button(ButtonSizes.Large)]
        public void RotateRight()
        {
            transform.rotation *= Quaternion.AngleAxis(10, Vector3.right);
        }

        /*
        private void OnDrawGizmos()
        {
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(gameObject.transform.position, DeltaQuat, transform.lossyScale);
            Gizmos.matrix = rotationMatrix;
            Debug.DrawRay(gameObject.transform.position, Vector3.one);
        }
        */

        private Vector3 EulerAnglesClamp(Quaternion from)
        {
            Vector3 delta = from.eulerAngles;

            if (delta.x > 180)
                delta.x -= 360;
            else if (delta.x < -180)
                delta.x += 360;

            if (delta.y > 180)
                delta.y -= 360;
            else if (delta.y < -180)
                delta.y += 360;

            if (delta.z > 180)
                delta.z -= 360;
            else if (delta.z < -180)
                delta.z += 360;

            return delta;
        }

        private Quaternion SeperateAxisY(Quaternion q)
        {
            float theta = Mathf.Atan2(q.y, q.w);

            return new Quaternion(0, Mathf.Sin(theta), 0, Mathf.Cos(theta));
        }

        private Quaternion SeperateAxisX(Quaternion q)
        {
            float theta = Mathf.Atan2(q.x, q.w);

            return new Quaternion(Mathf.Sin(theta), 0, 0, Mathf.Cos(theta));
        }

        private Quaternion SeperateAxisZ(Quaternion q)
        {
            float theta = Mathf.Atan2(q.z, q.w);

            return new Quaternion(0, 0, Mathf.Sin(theta), Mathf.Cos(theta));
        }
    }
}