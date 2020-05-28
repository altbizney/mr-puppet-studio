using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;



namespace MrPuppet
{
    public class PoseSpaceObjectTest : MonoBehaviour
    {

        private GameObject Clone1;
        private GameObject Clone2;
        Quaternion DeltaQuat;

        /*
        [InfoBox("DeltaEuler is quaternion subtraction, to euler conversion. DeltaClamped is quaternion subtraction to euler conversion, clamped. DeltaAngle~ is using the DeltaAngle method.")]

        [ReadOnly]
        public Vector3 DeltaEuler;

        [ReadOnly]
        public Vector3 DeltaClamped;

        [ReadOnly]
        public Vector3 DeltaAngleAttach;
        [ReadOnly]
        public Vector3 DeltaAnglePose1;
        [ReadOnly]
        public Vector3 DeltaAnglePose2;

        [ReadOnly]
        public Vector3 DeltaScore;

        [ReadOnly]
        public Vector3 DeltaScore2;
        */

        [Title("Poses")]
        [ReadOnly]
        public Quaternion Attach;
        [ReadOnly]
        public Quaternion Pose1;
        [ReadOnly]
        public Quaternion Pose2;

        [Title("First Scores")]
        [ReadOnly]
        public float ScoreX1;
        [ReadOnly]
        public float ScoreY1;
        [ReadOnly]
        public float ScoreZ1;

        [Title("Second Scores")]
        [ReadOnly]
        public float ScoreX2;
        [ReadOnly]
        public float ScoreY2;
        [ReadOnly]
        public float ScoreZ2;

        [Title("Total Scores")]
        [ReadOnly]
        public float ScoreTotal1;
        [ReadOnly]
        public float ScoreTotal2;


        private void Awake()
        {
            GameObject Prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/_Prefabs/0 - Debug/CubeColored.prefab", typeof(GameObject));

            Clone1 = Instantiate(Prefab, gameObject.transform.position + new Vector3(0, 2f, 0), gameObject.transform.rotation);
            Clone2 = Instantiate(Prefab, gameObject.transform.position + new Vector3(0, -2f, 0), gameObject.transform.rotation);

            SnapshotAttach();
            SnapshotPose1();
            SnapshotPose2();

            //transform.rotation *= Quaternion.AngleAxis(30, Vector3.up);
            //transform.rotation *= Quaternion.AngleAxis(30, Vector3.down);
        }

        void Update()
        {
            //DeltaQuat = gameObject.transform.localRotation * Quaternion.Inverse(Attach);

            //DeltaAngleAttach = PopulateVector(Attach);
            //DeltaAnglePose1 = PopulateVector(Pose1);
            //DeltaAnglePose2 = PopulateVector(Pose2);

            ScoreY1 = RemapAndClamp(gameObject.transform.localRotation.eulerAngles.y, Attach.eulerAngles.y, Pose1.eulerAngles.y, 0f, 1f);
            ScoreX1 = RemapAndClamp(gameObject.transform.localRotation.eulerAngles.x, Attach.eulerAngles.x, Pose1.eulerAngles.x, 0f, 1f);
            ScoreZ1 = RemapAndClamp(gameObject.transform.localRotation.eulerAngles.z, Attach.eulerAngles.z, Pose1.eulerAngles.z, 0f, 1f);
            ScoreTotal1 = ScoreY1 + ScoreX1 + ScoreZ1;

            ScoreY2 = RemapAndClamp(gameObject.transform.localRotation.eulerAngles.y, Attach.eulerAngles.y, Pose2.eulerAngles.y, 0f, 1f);
            ScoreX2 = RemapAndClamp(gameObject.transform.localRotation.eulerAngles.x, Attach.eulerAngles.x, Pose2.eulerAngles.x, 0f, 1f);
            ScoreZ2 = RemapAndClamp(gameObject.transform.localRotation.eulerAngles.z, Attach.eulerAngles.z, Pose2.eulerAngles.z, 0f, 1f);
            ScoreTotal2 = ScoreY2 + ScoreX2 + ScoreZ2; //RemapAndClamp(ScoreTotal2, 0f, , 0f, 1f);


            // DeltaClamped = EulerAnglesClamp(DeltaQuat);
            // DeltaEuler = DeltaQuat.eulerAngles;
        }

        /*
        private Vector3 PopulateVector(Quaternion SnapshotPose)
        {
            Vector3 Delta = new Vector3();

            Delta.y = Mathf.DeltaAngle(SnapshotPose.eulerAngles.y, gameObject.transform.localRotation.eulerAngles.y);
            Delta.x = Mathf.DeltaAngle(SnapshotPose.eulerAngles.x, gameObject.transform.localRotation.eulerAngles.x);
            Delta.z = Mathf.DeltaAngle(SnapshotPose.eulerAngles.z, gameObject.transform.localRotation.eulerAngles.z);

            return Delta;
        }
        */

        private float RemapAndClamp(float value, float from1, float to1, float from2, float to2)
        {
            if (from1 == to1)
                return to2;
            else
                return value.Remap(from1, to1, from2, to2).Clamp(from2, to2);
        }

        [Button(ButtonSizes.Large)]
        public void SnapshotAttach()
        {
            Attach = gameObject.transform.localRotation;
        }

        [HorizontalGroup("Poses")]
        [Button(ButtonSizes.Large)]
        public void SnapshotPose1()
        {
            Pose1 = gameObject.transform.localRotation;
            Clone1.transform.rotation = gameObject.transform.rotation;
        }

        [HorizontalGroup("Poses")]
        [Button(ButtonSizes.Large)]
        public void SnapshotPose2()
        {
            Pose2 = gameObject.transform.localRotation;
            Clone2.transform.rotation = gameObject.transform.rotation;
        }

        [HorizontalGroup("UpDown")]
        [Button(ButtonSizes.Large)]
        public void RotateUp()
        {
            transform.rotation *= Quaternion.AngleAxis(10, Vector3.up);
        }

        [HorizontalGroup("UpDown")]
        [Button(ButtonSizes.Large)]
        public void RotateDown()
        {
            transform.rotation *= Quaternion.AngleAxis(10, Vector3.down);
        }

        [HorizontalGroup("LeftRight")]
        [Button(ButtonSizes.Large)]
        public void RotateLeft()
        {
            transform.rotation *= Quaternion.AngleAxis(10, Vector3.left);
        }

        [HorizontalGroup("LeftRight")]
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
         */

    }
}