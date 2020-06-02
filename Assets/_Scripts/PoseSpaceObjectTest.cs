using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;



namespace MrPuppet
{
    public class PoseSpaceObjectTest : MonoBehaviour
    {
        [Title("Pose")]
        [ReadOnly]
        public Vector3 PoseWrist;
        [ReadOnly]
        public Vector3 PoseElbow;
        [ReadOnly]
        public Vector3 PoseShoulder;

        [Title("Joints")]
        [ReadOnly]
        public Vector3 JointWrist;
        [ReadOnly]
        public Vector3 JointElbow;
        [ReadOnly]
        public Vector3 JointShoulder;

        [Title("Total Scores")]
        [ReadOnly]
        public float NorthScore;
        [ReadOnly]
        public float SouthScore;
        [ReadOnly]
        public float EastScore;
        [ReadOnly]
        public float WestScore;

        [Title("Formula - live_origin_dela_z / pose_attach_delta_z = score")]
        [ReadOnly]
        public string[] FormulaX = new string[3];
        [ReadOnly]
        public string[] FormulaY = new string[3];
        [ReadOnly]
        public string[] FormulaZ = new string[3];

        private class RotationAxis
        {
            public Quaternion YAxis;
            public Quaternion XAxis;
            public Quaternion ZAxis;
        }

        private MrPuppetDataMapper DataMapper;
        private GameObject North;
        private GameObject South;
        private GameObject East;
        private GameObject West;


        private GameObject[] JointsArm = new GameObject[3];
        private RotationAxis[] ArmAxes = new RotationAxis[3];

        private GameObject[] NorthPoseJoints = new GameObject[3];
        private GameObject[] SouthPoseJoints = new GameObject[3];
        private GameObject[] EastPoseJoints = new GameObject[3];
        private GameObject[] WestPoseJoints = new GameObject[3];
        private RotationAxis[] NorthPoseAxes = new RotationAxis[3];
        private RotationAxis[] SouthPoseAxes = new RotationAxis[3];
        private RotationAxis[] EastPoseAxes = new RotationAxis[3];
        private RotationAxis[] WestPoseAxes = new RotationAxis[3];

        private RotationAxis ZeroAxes = new RotationAxis();

        private void Awake()
        {
            DataMapper = FindObjectOfType<MrPuppetDataMapper>();

            GameObject Prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/_Prefabs/0 - Debug/CubeColored.prefab", typeof(GameObject));

            JointsArm[0] = gameObject;
            for (int j = 1; j < JointsArm.Length; j++)
                JointsArm[j] = Instantiate(Prefab, gameObject.transform.position + new Vector3(0, -(j * 1.5f), 0), gameObject.transform.rotation);

            for (int j = 0; j < NorthPoseJoints.Length; j++)
            {
                NorthPoseJoints[j] = Instantiate(Prefab, gameObject.transform.position + new Vector3(0f, -(j * 1.5f), 2.5f), gameObject.transform.rotation);
                SouthPoseJoints[j] = Instantiate(Prefab, gameObject.transform.position + new Vector3(0f, -(j * 1.5f), -2.5f), gameObject.transform.rotation);
                EastPoseJoints[j] = Instantiate(Prefab, gameObject.transform.position + new Vector3(2.5f, -(j * 1.5f), 0f), gameObject.transform.rotation);
                WestPoseJoints[j] = Instantiate(Prefab, gameObject.transform.position + new Vector3(-2.5f, -(j * 1.5f), 0f), gameObject.transform.rotation);
            }

            for (int p = 0; p < NorthPoseAxes.Length; p++)
            {
                NorthPoseAxes[p] = new RotationAxis();
                SouthPoseAxes[p] = new RotationAxis();
                EastPoseAxes[p] = new RotationAxis();
                WestPoseAxes[p] = new RotationAxis();

                ArmAxes[p] = new RotationAxis();
            }

            SnapshotNorthPose();
            SnapshotSouthPose();
            SnapshotEastPose();
            SnapshotWestPose();

            North = GameObject.Find("North");
            South = GameObject.Find("South");
            East = GameObject.Find("East");
            West = GameObject.Find("West");
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

            JointsArm[0].transform.localRotation = RotationDeltaFromAttachWrist();
            JointsArm[1].transform.localRotation = RotationDeltaFromAttachElbow();
            JointsArm[2].transform.localRotation = RotationDeltaFromAttachShoulder();

            for (int p = 0; p < NorthPoseJoints.Length; p++)
            {
                NorthPoseAxes[p].YAxis = Quaternion.AngleAxis(NorthPoseJoints[p].transform.localRotation.eulerAngles.y, NorthPoseJoints[p].transform.up);
                NorthPoseAxes[p].XAxis = Quaternion.AngleAxis(NorthPoseJoints[p].transform.localRotation.eulerAngles.x, NorthPoseJoints[p].transform.right);
                NorthPoseAxes[p].ZAxis = Quaternion.AngleAxis(NorthPoseJoints[p].transform.localRotation.eulerAngles.z, NorthPoseJoints[p].transform.forward);

                SouthPoseAxes[p].YAxis = Quaternion.AngleAxis(SouthPoseJoints[p].transform.localRotation.eulerAngles.y, SouthPoseJoints[p].transform.up);
                SouthPoseAxes[p].XAxis = Quaternion.AngleAxis(SouthPoseJoints[p].transform.localRotation.eulerAngles.x, SouthPoseJoints[p].transform.right);
                SouthPoseAxes[p].ZAxis = Quaternion.AngleAxis(SouthPoseJoints[p].transform.localRotation.eulerAngles.z, SouthPoseJoints[p].transform.forward);

                EastPoseAxes[p].YAxis = Quaternion.AngleAxis(EastPoseJoints[p].transform.localRotation.eulerAngles.y, EastPoseJoints[p].transform.up);
                EastPoseAxes[p].XAxis = Quaternion.AngleAxis(EastPoseJoints[p].transform.localRotation.eulerAngles.x, EastPoseJoints[p].transform.right);
                EastPoseAxes[p].ZAxis = Quaternion.AngleAxis(EastPoseJoints[p].transform.localRotation.eulerAngles.z, EastPoseJoints[p].transform.forward);

                WestPoseAxes[p].YAxis = Quaternion.AngleAxis(WestPoseJoints[p].transform.localRotation.eulerAngles.y, WestPoseJoints[p].transform.up);
                WestPoseAxes[p].XAxis = Quaternion.AngleAxis(WestPoseJoints[p].transform.localRotation.eulerAngles.x, WestPoseJoints[p].transform.right);
                WestPoseAxes[p].ZAxis = Quaternion.AngleAxis(WestPoseJoints[p].transform.localRotation.eulerAngles.z, WestPoseJoints[p].transform.forward);

                ArmAxes[p].YAxis = Quaternion.AngleAxis(JointsArm[p].transform.localRotation.eulerAngles.y, JointsArm[p].transform.up);
                ArmAxes[p].XAxis = Quaternion.AngleAxis(JointsArm[p].transform.localRotation.eulerAngles.x, JointsArm[p].transform.right);
                ArmAxes[p].ZAxis = Quaternion.AngleAxis(JointsArm[p].transform.localRotation.eulerAngles.z, JointsArm[p].transform.forward);
            }

            ZeroAxes.YAxis = Quaternion.AngleAxis(0f, Vector3.up);
            ZeroAxes.XAxis = Quaternion.AngleAxis(0f, Vector3.right);
            ZeroAxes.ZAxis = Quaternion.AngleAxis(0f, Vector3.forward);

            NorthScore = PopulateScores(NorthPoseAxes);
            SouthScore = PopulateScores(SouthPoseAxes);
            EastScore = PopulateScores(EastPoseAxes);
            WestScore = PopulateScores(WestPoseAxes);

            North.transform.localScale = new Vector3(1f, NorthScore, 1f);
            South.transform.localScale = new Vector3(1f, SouthScore, 1f);
            East.transform.localScale = new Vector3(1f, EastScore, 1f);
            West.transform.localScale = new Vector3(1f, WestScore, 1f);

            JointWrist = JointsArm[0].transform.localRotation.eulerAngles;
            JointElbow = JointsArm[1].transform.localRotation.eulerAngles;
            JointShoulder = JointsArm[2].transform.localRotation.eulerAngles;
        }

        private float PopulateScores(RotationAxis[] PoseAxes)
        {
            Vector3[] Scores = new Vector3[3];
            for (int p = 0; p < NorthPoseJoints.Length; p++)
            {
                //Having these initialized like 9 times each frame is no good
                float pose_attach_delta_y = Quaternion.Angle(ZeroAxes.YAxis, PoseAxes[p].YAxis);
                float pose_attach_delta_x = Quaternion.Angle(ZeroAxes.XAxis, PoseAxes[p].XAxis);
                float pose_attach_delta_z = Quaternion.Angle(ZeroAxes.ZAxis, PoseAxes[p].ZAxis);

                float live_origin_delta_y = Quaternion.Angle(ZeroAxes.YAxis, ArmAxes[p].YAxis);
                float live_origin_delta_x = Quaternion.Angle(ZeroAxes.XAxis, ArmAxes[p].XAxis);
                float live_origin_delta_z = Quaternion.Angle(ZeroAxes.ZAxis, ArmAxes[p].ZAxis);

                Scores[p].y = (live_origin_delta_y / pose_attach_delta_y).Clamp(0f, 1f);
                Scores[p].x = (live_origin_delta_x / pose_attach_delta_x).Clamp(0f, 1f);
                Scores[p].z = (live_origin_delta_z / pose_attach_delta_z).Clamp(0f, 1f);

                FormulaX[p] = live_origin_delta_x + " / " + pose_attach_delta_x + " = " + live_origin_delta_x / pose_attach_delta_x;
                FormulaY[p] = live_origin_delta_y + " / " + pose_attach_delta_y + " = " + live_origin_delta_y / pose_attach_delta_y;
                FormulaZ[p] = live_origin_delta_z + " / " + pose_attach_delta_z + " = " + live_origin_delta_z / pose_attach_delta_z;
            }


            float ScoreTotal = Scores[0].x + Scores[0].y + Scores[0].z + Scores[1].x + Scores[1].y + Scores[1].z + Scores[2].x + Scores[2].y + Scores[2].z;
            ScoreTotal = Remap(ScoreTotal, 0f, 9f, 0f, 1f);

            return ScoreTotal;
        }

        public float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return Mathf.Clamp((value - from1) / (to1 - from1) * (to2 - from2) + from2, from2, to2);
        }

        [Button(ButtonSizes.Medium)]
        [HorizontalGroup("ButtonsNS")]
        public void SnapshotNorthPose()
        {
            for (int p = 0; p < NorthPoseJoints.Length; p++)
            {
                Quaternion totalQ = Quaternion.Euler(ArmAxes[p].XAxis.eulerAngles.x, ArmAxes[p].YAxis.eulerAngles.y, ArmAxes[p].ZAxis.eulerAngles.z);
                NorthPoseJoints[p].transform.rotation = totalQ;
            }
        }

        [Button(ButtonSizes.Medium)]
        [HorizontalGroup("ButtonsNS")]
        public void SnapshotSouthPose()
        {
            for (int p = 0; p < NorthPoseJoints.Length; p++)
            {
                Quaternion totalQ = Quaternion.Euler(ArmAxes[p].XAxis.eulerAngles.x, ArmAxes[p].YAxis.eulerAngles.y, ArmAxes[p].ZAxis.eulerAngles.z);
                SouthPoseJoints[p].transform.rotation = totalQ;
            }
        }

        [Button(ButtonSizes.Medium)]
        [HorizontalGroup("ButtonsEW")]
        public void SnapshotEastPose()
        {
            for (int p = 0; p < NorthPoseJoints.Length; p++)
            {
                Quaternion totalQ = Quaternion.Euler(ArmAxes[p].XAxis.eulerAngles.x, ArmAxes[p].YAxis.eulerAngles.y, ArmAxes[p].ZAxis.eulerAngles.z);
                EastPoseJoints[p].transform.rotation = totalQ;
            }
        }

        [Button(ButtonSizes.Medium)]
        [HorizontalGroup("ButtonsEW")]
        public void SnapshotWestPose()
        {
            for (int p = 0; p < NorthPoseJoints.Length; p++)
            {
                Quaternion totalQ = Quaternion.Euler(ArmAxes[p].XAxis.eulerAngles.x, ArmAxes[p].YAxis.eulerAngles.y, ArmAxes[p].ZAxis.eulerAngles.z);
                WestPoseJoints[p].transform.rotation = totalQ;
            }

            PoseWrist = WestPoseJoints[0].transform.localRotation.eulerAngles;
            PoseElbow = WestPoseJoints[1].transform.localRotation.eulerAngles;
            PoseShoulder = WestPoseJoints[2].transform.localRotation.eulerAngles;
        }

        /*
        private float RemapAndClamp(float value, float from1, float to1, float from2, float to2)
        {
            if (from1 == to1)
                return to2;
            else
                return value.Remap(from1, to1, from2, to2).Clamp(from2, to2);
        }
        */
    }
}