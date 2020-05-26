using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


namespace MrPuppet
{
    public class PoseSpaceObjectTest : MonoBehaviour
    {

        Quaternion AwakeSnapshot;
        Quaternion DeltaQuat;

        [ReadOnly]
        public Vector3 DeltaEuler;
        [ReadOnly]
        public Vector3 DeltaSeperate;

        void Awake()
        {
            AwakeSnapshot = gameObject.transform.localRotation;
            //transform.rotation *= Quaternion.AngleAxis(30, Vector3.up);
        }

        void Update()
        {
            DeltaQuat = gameObject.transform.localRotation * Quaternion.Inverse(AwakeSnapshot);

            DeltaEuler = DeltaQuat.eulerAngles;

            DeltaSeperate.y = SeperateAxisY(DeltaQuat).eulerAngles.y;
            DeltaSeperate.x = SeperateAxisX(DeltaQuat).eulerAngles.x;
            DeltaSeperate.z = SeperateAxisZ(DeltaQuat).eulerAngles.z;

        }

        /*
        private void OnDrawGizmos()
        {
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(gameObject.transform.position, DeltaQuat, transform.lossyScale);
            Gizmos.matrix = rotationMatrix;
            Debug.DrawRay(gameObject.transform.position, Vector3.one);
        }
        */

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