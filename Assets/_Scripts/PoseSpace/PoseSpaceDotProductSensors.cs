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
                float[] DotProductValuesLive = new float[3];
                float[] DotProductValuesAttach = new float[3];

                DotProductValuesLive[0] = Vector3.Dot(JointsArm[p].transform.forward, Pose[p].transform.forward);
                DotProductValuesLive[1] = Vector3.Dot(JointsArm[p].transform.up, Pose[p].transform.up);
                DotProductValuesLive[2] = Vector3.Dot(JointsArm[p].transform.right, Pose[p].transform.right);

                DotProductValuesAttach[0] = Vector3.Dot(Quaternion.identity * Vector3.forward, Pose[p].transform.forward);
                DotProductValuesAttach[1] = Vector3.Dot(Quaternion.identity * Vector3.up, Pose[p].transform.up);
                DotProductValuesAttach[2] = Vector3.Dot(Quaternion.identity * Vector3.right, Pose[p].transform.right);

                DotProductValuesLive[0] = Remap(DotProductValuesLive[0], DotProductValuesAttach[0], 1f, 0f, 1f);
                DotProductValuesLive[1] = Remap(DotProductValuesLive[1], DotProductValuesAttach[1], 1f, 0f, 1f);
                DotProductValuesLive[2] = Remap(DotProductValuesLive[2], DotProductValuesAttach[2], 1f, 0f, 1f);

                ScoreTotal += DotProductValuesLive[0] + DotProductValuesLive[1] + DotProductValuesLive[2];
            }

            ScoreTotal = ScoreTotal / 9;//Remap(ScoreTotal, 0f, 9f, 0f, 1f);

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
