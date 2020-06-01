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
        private MrPuppetDataMapper DataMapper;
        Quaternion DeltaQuat;

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

        [Title("Total Scores")]
        [ReadOnly]
        public float ScoreTotal1;
        [ReadOnly]
        public float ScoreTotal2;

        [Title("Formula - live_origin_dela_z / pose_attach_delta_z = score")]
        [ReadOnly]
        public string FormulaX;
        [ReadOnly]
        public string FormulaY;
        [ReadOnly]
        public string FormulaZ;

        private GameObject[] Joints = new GameObject[3];
        private GameObject[] JointsPose = new GameObject[3];

        private class RotationPose
        {
            public Quaternion YAxis;
            public Quaternion XAxis;
            public Quaternion ZAxis;
        }

        private RotationPose[] Poses = new RotationPose[3];
        private RotationPose[] IsolateJoiints = new RotationPose[3];
        private RotationPose ZeroAxis = new RotationPose();
        public Vector3 ScoreWrist;
        public Vector3 ScoreElbow;
        public Vector3 ScoreShoulder;

        private void Awake()
        {
            DataMapper = FindObjectOfType<MrPuppetDataMapper>();

            GameObject Prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/_Prefabs/0 - Debug/CubeColored.prefab", typeof(GameObject));

            Joints[0] = gameObject;
            for (int j = 1; j < JointsPose.Length; j++)
                Joints[j] = Instantiate(Prefab, gameObject.transform.position + new Vector3(0, -j, 0), gameObject.transform.rotation);

            for (int j = 0; j < JointsPose.Length; j++)
                JointsPose[j] = Instantiate(Prefab, gameObject.transform.position + new Vector3(1, -j, 0), gameObject.transform.rotation);

            for (int p = 0; p < JointsPose.Length; p++)
            {
                Poses[p] = new RotationPose();
                IsolateJoiints[p] = new RotationPose();
            }

            SnapshotPose1();
        }

        private Quaternion RotationDeltaFromAttachWrist()
        {
            return DataMapper.WristJoint.rotation * Quaternion.Inverse(DataMapper.AttachPose.WristRotation);
        }

        private Quaternion RotationDeltaFromAttachElbow()
        {
            return DataMapper.ElbowJoint.rotation * Quaternion.Inverse(DataMapper.AttachPose.ElbowRotation);
        }

        private Quaternion RotationDeltaFromAttachShoulder()
        {
            return DataMapper.ShoulderJoint.rotation * Quaternion.Inverse(DataMapper.AttachPose.ShoulderRotation);
        }

        void Update()
        {
            if (!DataMapper.AttachPoseSet) return;

            Joints[0].transform.localRotation = RotationDeltaFromAttachWrist();
            Joints[1].transform.localRotation = RotationDeltaFromAttachElbow();
            Joints[2].transform.localRotation = RotationDeltaFromAttachShoulder();

            for (int p = 0; p < JointsPose.Length; p++)
            {
                Poses[p].YAxis = Quaternion.AngleAxis(JointsPose[p].transform.localRotation.eulerAngles.y, JointsPose[p].transform.up);
                Poses[p].XAxis = Quaternion.AngleAxis(JointsPose[p].transform.localRotation.eulerAngles.x, JointsPose[p].transform.right);
                Poses[p].ZAxis = Quaternion.AngleAxis(JointsPose[p].transform.localRotation.eulerAngles.z, JointsPose[p].transform.forward);
            }

            for (int p = 0; p < Joints.Length; p++)
            {
                IsolateJoiints[p].YAxis = Quaternion.AngleAxis(Joints[p].transform.localRotation.eulerAngles.y, Joints[p].transform.up);
                IsolateJoiints[p].XAxis = Quaternion.AngleAxis(Joints[p].transform.localRotation.eulerAngles.x, Joints[p].transform.right);
                IsolateJoiints[p].ZAxis = Quaternion.AngleAxis(Joints[p].transform.localRotation.eulerAngles.z, Joints[p].transform.forward);
            }

            ZeroAxis.YAxis = Quaternion.AngleAxis(0f, Vector3.up);
            ZeroAxis.XAxis = Quaternion.AngleAxis(0f, Vector3.right);
            ZeroAxis.ZAxis = Quaternion.AngleAxis(0f, Vector3.forward);

            ScoreWrist = PopulateScores(0);
            ScoreElbow = PopulateScores(1);
            ScoreShoulder = PopulateScores(2);

            //FormulaX = live_origin_dela_x + " / " + pose_attach_delta_x + " = " + live_origin_dela_x / pose_attach_delta_x;
            //FormulaY = live_origin_dela_y + " / " + pose_attach_delta_y + " = " + live_origin_dela_y / pose_attach_delta_y;
            //FormulaZ = live_origin_dela_z + " / " + pose_attach_delta_z + " = " + live_origin_dela_z / pose_attach_delta_z;
        }

        private Vector3 PopulateScores(int index)
        {

            float pose_attach_delta_y = Quaternion.Angle(ZeroAxis.YAxis, Poses[index].YAxis);
            float pose_attach_delta_x = Quaternion.Angle(ZeroAxis.XAxis, Poses[index].XAxis);
            float pose_attach_delta_z = Quaternion.Angle(ZeroAxis.ZAxis, Poses[index].ZAxis);

            float live_origin_dela_y = Quaternion.Angle(ZeroAxis.YAxis, IsolateJoiints[index].YAxis);
            float live_origin_dela_x = Quaternion.Angle(ZeroAxis.XAxis, IsolateJoiints[index].YAxis);
            float live_origin_dela_z = Quaternion.Angle(ZeroAxis.ZAxis, IsolateJoiints[index].YAxis);

            Vector3 Score;
            Score.y = (live_origin_dela_y / pose_attach_delta_y).Clamp(0f, 1f);
            Score.x = (live_origin_dela_x / pose_attach_delta_x).Clamp(0f, 1f);
            Score.z = (live_origin_dela_z / pose_attach_delta_z).Clamp(0f, 1f);

            return Score;
        }

        private float RemapAndClamp(float value, float from1, float to1, float from2, float to2)
        {
            if (from1 == to1)
                return to2;
            else
                return value.Remap(from1, to1, from2, to2).Clamp(from2, to2);
        }

        [Button(ButtonSizes.Large)]
        public void SnapshotPose1()
        {
            for (int p = 0; p < JointsPose.Length; p++)
            {
                Poses[p] = IsolateJoiints[p];
            }

            for (int p = 0; p < JointsPose.Length; p++)
            {
                Quaternion totalQ = Quaternion.Euler(IsolateJoiints[p].XAxis.eulerAngles.x, IsolateJoiints[p].YAxis.eulerAngles.y, IsolateJoiints[p].ZAxis.eulerAngles.z);

                JointsPose[p].transform.rotation = totalQ;
            }
        }
    }
}