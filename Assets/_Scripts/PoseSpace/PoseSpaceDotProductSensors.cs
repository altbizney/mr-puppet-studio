using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

namespace MrPuppet
{
    public class PoseSpaceDotProductSensors : MonoBehaviour
    {

        [Title("Total Scores")]
        [ReadOnly]
        public float NorthScore;
        [ReadOnly]
        public float SouthScore;
        [ReadOnly]
        public float EastScore;
        [ReadOnly]
        public float WestScore;

        private MrPuppetDataMapper DataMapper;
        private GameObject North;
        private GameObject South;
        private GameObject East;
        private GameObject West;

        private GameObject[] JointsArm = new GameObject[3];

        private GameObject[] NorthPoseJoints = new GameObject[3];
        private GameObject[] SouthPoseJoints = new GameObject[3];
        private GameObject[] EastPoseJoints = new GameObject[3];
        private GameObject[] WestPoseJoints = new GameObject[3];

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

            NorthScore = PopulateScores(NorthPoseJoints);
            SouthScore = PopulateScores(SouthPoseJoints);
            EastScore = PopulateScores(EastPoseJoints);
            WestScore = PopulateScores(WestPoseJoints);

            if (!float.IsNaN(NorthScore))
                North.transform.localScale = new Vector3(1f, NorthScore, 1f);
            if (!float.IsNaN(SouthScore))
                South.transform.localScale = new Vector3(1f, SouthScore, 1f);
            if (!float.IsNaN(EastScore))
                East.transform.localScale = new Vector3(1f, EastScore, 1f);
            if (!float.IsNaN(WestScore))
                West.transform.localScale = new Vector3(1f, WestScore, 1f);
        }

        private float PopulateScores(GameObject[] Pose)
        {
            float ScoreTotal = 0;
            for (int p = 0; p < NorthPoseJoints.Length; p++)
            {
                float[] DotProductValues = new float[3];

                DotProductValues[0] = Vector3.Dot(JointsArm[p].transform.forward, Pose[p].transform.forward);
                DotProductValues[1] = Vector3.Dot(JointsArm[p].transform.up, Pose[p].transform.up);
                DotProductValues[2] = Vector3.Dot(JointsArm[p].transform.right, Pose[p].transform.right);

                DotProductValues[0] = Remap(DotProductValues[0], -1f, 1f, 0f, 1f);
                DotProductValues[1] = Remap(DotProductValues[1], -1f, 1f, 0f, 1f);
                DotProductValues[2] = Remap(DotProductValues[2], -1f, 1f, 0f, 1f);

                ScoreTotal += DotProductValues[0] + DotProductValues[1] + DotProductValues[2];
            }

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
                NorthPoseJoints[p].transform.localRotation = JointsArm[p].transform.localRotation;
            }
        }

        [Button(ButtonSizes.Medium)]
        [HorizontalGroup("ButtonsNS")]
        public void SnapshotSouthPose()
        {
            for (int p = 0; p < NorthPoseJoints.Length; p++)
            {
                SouthPoseJoints[p].transform.localRotation = JointsArm[p].transform.localRotation;
            }
        }

        [Button(ButtonSizes.Medium)]
        [HorizontalGroup("ButtonsEW")]
        public void SnapshotEastPose()
        {
            for (int p = 0; p < NorthPoseJoints.Length; p++)
            {
                EastPoseJoints[p].transform.localRotation = JointsArm[p].transform.localRotation;
            }
        }

        [Button(ButtonSizes.Medium)]
        [HorizontalGroup("ButtonsEW")]
        public void SnapshotWestPose()
        {
            for (int p = 0; p < NorthPoseJoints.Length; p++)
            {
                WestPoseJoints[p].transform.localRotation = JointsArm[p].transform.localRotation;
            }
        }

    }
}
