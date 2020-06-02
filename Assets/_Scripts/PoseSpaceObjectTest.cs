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
        public Vector3 ScoreWrist;
        [ReadOnly]
        public Vector3 ScoreElbow;
        [ReadOnly]
        public Vector3 ScoreShoulder;
        [ReadOnly]
        public float ScoreTotal;

        private GameObject North;

        /*
        [Title("Formula - live_origin_dela_z / pose_attach_delta_z = score")]
        [ReadOnly]
        public string FormulaX;
        [ReadOnly]
        public string FormulaY;
        [ReadOnly]
        public string FormulaZ;
        */

        private class RotationAxis
        {
            public Quaternion YAxis;
            public Quaternion XAxis;
            public Quaternion ZAxis;
        }

        private MrPuppetDataMapper DataMapper;

        private GameObject[] JointsArm = new GameObject[3];
        private GameObject[] JointsPose = new GameObject[3];

        private RotationAxis[] PoseAxes = new RotationAxis[3];
        private RotationAxis[] ArmAxes = new RotationAxis[3];
        private RotationAxis ZeroAxes = new RotationAxis();

        private void Awake()
        {
            DataMapper = FindObjectOfType<MrPuppetDataMapper>();

            GameObject Prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/_Prefabs/0 - Debug/CubeColored.prefab", typeof(GameObject));

            JointsArm[0] = gameObject;
            for (int j = 1; j < JointsArm.Length; j++)
                JointsArm[j] = Instantiate(Prefab, gameObject.transform.position + new Vector3(0, -(j * 1.5f), 0), gameObject.transform.rotation);

            for (int j = 0; j < JointsPose.Length; j++)
                JointsPose[j] = Instantiate(Prefab, gameObject.transform.position + new Vector3(2.5f, -(j * 1.5f), 0), gameObject.transform.rotation);

            for (int p = 0; p < PoseAxes.Length; p++)
            {
                PoseAxes[p] = new RotationAxis();
                ArmAxes[p] = new RotationAxis();
            }

            SnapshotPose();

            North = GameObject.Find("N Marker");
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

            for (int p = 0; p < JointsPose.Length; p++)
            {
                PoseAxes[p].YAxis = Quaternion.AngleAxis(JointsPose[p].transform.localRotation.eulerAngles.y, JointsPose[p].transform.up);
                PoseAxes[p].XAxis = Quaternion.AngleAxis(JointsPose[p].transform.localRotation.eulerAngles.x, JointsPose[p].transform.right);
                PoseAxes[p].ZAxis = Quaternion.AngleAxis(JointsPose[p].transform.localRotation.eulerAngles.z, JointsPose[p].transform.forward);
            }

            for (int p = 0; p < JointsArm.Length; p++)
            {
                ArmAxes[p].YAxis = Quaternion.AngleAxis(JointsArm[p].transform.localRotation.eulerAngles.y, JointsArm[p].transform.up);
                ArmAxes[p].XAxis = Quaternion.AngleAxis(JointsArm[p].transform.localRotation.eulerAngles.x, JointsArm[p].transform.right);
                ArmAxes[p].ZAxis = Quaternion.AngleAxis(JointsArm[p].transform.localRotation.eulerAngles.z, JointsArm[p].transform.forward);
            }

            Debug.Log("Not equal " + Quaternion.AngleAxis(JointsArm[0].transform.localRotation.eulerAngles.y, JointsArm[0].transform.up) + " " + Quaternion.AngleAxis(JointsPose[0].transform.localRotation.eulerAngles.y, JointsPose[0].transform.up));
            Debug.Log(ArmAxes[0].YAxis.eulerAngles.y + " " + PoseAxes[0].YAxis.eulerAngles.y);

            ZeroAxes.YAxis = Quaternion.AngleAxis(0f, Vector3.up);
            ZeroAxes.XAxis = Quaternion.AngleAxis(0f, Vector3.right);
            ZeroAxes.ZAxis = Quaternion.AngleAxis(0f, Vector3.forward);

            ScoreWrist = PopulateScores(0);
            ScoreElbow = PopulateScores(1);
            ScoreShoulder = PopulateScores(2);
            ScoreTotal = ScoreWrist.x + ScoreWrist.y + ScoreWrist.z + ScoreElbow.x + ScoreElbow.y + ScoreElbow.z + ScoreShoulder.x + ScoreShoulder.y + ScoreShoulder.z;
            ScoreTotal = Remap(ScoreTotal, 0f, 9f, 0f, 1f);

            North.transform.localScale = new Vector3(1f, 6 * ScoreTotal, 1f);

            JointWrist = JointsArm[0].transform.localRotation.eulerAngles;
            JointElbow = JointsArm[1].transform.localRotation.eulerAngles;
            JointShoulder = JointsArm[2].transform.localRotation.eulerAngles;

            //FormulaX = live_origin_dela_x + " / " + pose_attach_delta_x + " = " + live_origin_dela_x / pose_attach_delta_x;
            //FormulaY = live_origin_dela_y + " / " + pose_attach_delta_y + " = " + live_origin_dela_y / pose_attach_delta_y;
            //FormulaZ = live_origin_dela_z + " / " + pose_attach_delta_z + " = " + live_origin_dela_z / pose_attach_delta_z;
        }

        private Vector3 PopulateScores(int index)
        {

            float pose_attach_delta_y = Quaternion.Angle(ZeroAxes.YAxis, PoseAxes[index].YAxis);
            float pose_attach_delta_x = Quaternion.Angle(ZeroAxes.XAxis, PoseAxes[index].XAxis);
            float pose_attach_delta_z = Quaternion.Angle(ZeroAxes.ZAxis, PoseAxes[index].ZAxis);

            float live_origin_delta_y = Quaternion.Angle(ZeroAxes.YAxis, ArmAxes[index].YAxis);
            float live_origin_delta_x = Quaternion.Angle(ZeroAxes.XAxis, ArmAxes[index].XAxis);
            float live_origin_delta_z = Quaternion.Angle(ZeroAxes.ZAxis, ArmAxes[index].ZAxis);

            Vector3 Score;
            Score.y = (live_origin_delta_y / pose_attach_delta_y).Clamp(0f, 1f);
            Score.x = (live_origin_delta_x / pose_attach_delta_x).Clamp(0f, 1f);
            Score.z = (live_origin_delta_z / pose_attach_delta_z).Clamp(0f, 1f);

            return Score;
        }

        public float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return Mathf.Clamp((value - from1) / (to1 - from1) * (to2 - from2) + from2, from2, to2);
        }

        [Button(ButtonSizes.Large)]
        public void SnapshotPose()
        {
            for (int p = 0; p < JointsPose.Length; p++)
            {
                Quaternion totalQ = Quaternion.Euler(ArmAxes[p].XAxis.eulerAngles.x, ArmAxes[p].YAxis.eulerAngles.y, ArmAxes[p].ZAxis.eulerAngles.z);
                JointsPose[p].transform.rotation = totalQ;
            }

            PoseWrist = JointsPose[0].transform.localRotation.eulerAngles;
            PoseElbow = JointsPose[1].transform.localRotation.eulerAngles;
            PoseShoulder = JointsPose[2].transform.localRotation.eulerAngles;
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