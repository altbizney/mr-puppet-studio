using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using System.Linq.Expressions;
using System;

namespace MrPuppet
{
    public class PoseSpaceSigned : MonoBehaviour
    {

        private MrPuppetDataMapper DataMapper;
        private GameObject North;
        private GameObject South;
        private GameObject East;
        private GameObject West;

        private Quaternion poseNorth;
        private Quaternion poseSouth;
        private Quaternion poseEast;
        private Quaternion poseWest;

        [ReadOnly]
        public float NorthTotalScore;
        [ReadOnly]
        public float SouthTotalScore;
        [ReadOnly]
        public float EastTotalScore;
        [ReadOnly]
        public float WestTotalScore;

        [ReadOnly]
        public Vector3 poseNorthSeperated;
        [ReadOnly]
        public Vector3 poseSouthSeperated;
        [ReadOnly]
        public Vector3 poseEastSeperated;
        [ReadOnly]
        public Vector3 poseWestSeperated;
        [ReadOnly]
        public float deltaLiveOriginXZ;
        [ReadOnly]
        public float deltaLiveOriginXY;
        [ReadOnly]
        public float deltaLiveOriginYZ;
        //public Vector3 liveSeperated;
        //public Vector3 zeroSeperated;

        [ReadOnly]
        public float liveXZ;
        [ReadOnly]
        public float liveXY;
        [ReadOnly]
        public float liveYZ;

        [ReadOnly]
        public float zeroXZ;
        [ReadOnly]
        public float zeroXY;
        [ReadOnly]
        public float zeroYZ;

        private void Awake()
        {
            DataMapper = FindObjectOfType<MrPuppetDataMapper>();

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

        void Update()
        {
            if (!DataMapper.AttachPoseSet) return;

            gameObject.transform.localRotation = RotationDeltaFromAttachWrist();

            var liveForward = gameObject.transform.rotation * Vector3.forward;
            var liveUp = gameObject.transform.rotation * Vector3.up;
            var zeroForward = Quaternion.identity * Vector3.forward;
            var zeroUp = Quaternion.identity * Vector3.up;

            var poseNorthForward = poseNorth * Vector3.forward;
            var poseSouthForward = poseSouth * Vector3.forward;
            var poseEastForward = poseEast * Vector3.forward;
            var poseWestForward = poseWest * Vector3.forward;

            var poseNorthUp = poseNorth * Vector3.up;
            var poseSouthUp = poseSouth * Vector3.up;
            var poseEastUp = poseEast * Vector3.up;
            var poseWestUp = poseWest * Vector3.up;

            liveXZ = Mathf.Atan2(liveForward.x, liveForward.z) * Mathf.Rad2Deg;
            liveXY = Mathf.Atan2(liveUp.x, liveUp.y) * Mathf.Rad2Deg;
            liveYZ = Mathf.Atan2(liveUp.y, liveUp.z) * Mathf.Rad2Deg;

            zeroXZ = Mathf.Atan2(zeroForward.y, zeroForward.z) * Mathf.Rad2Deg;
            zeroXY = Mathf.Atan2(zeroUp.x, zeroUp.y) * Mathf.Rad2Deg;
            zeroYZ = Mathf.Atan2(zeroUp.y, zeroUp.z) * Mathf.Rad2Deg;

            poseNorthSeperated.x = Mathf.Atan2(poseNorthForward.x, poseNorthForward.z) * Mathf.Rad2Deg;
            poseNorthSeperated.y = Mathf.Atan2(poseNorthUp.x, poseNorthUp.y) * Mathf.Rad2Deg;
            poseNorthSeperated.z = Mathf.Atan2(poseNorthUp.y, poseNorthUp.z) * Mathf.Rad2Deg;

            poseSouthSeperated.x = Mathf.Atan2(poseSouthForward.x, poseSouthForward.z) * Mathf.Rad2Deg;
            poseSouthSeperated.y = Mathf.Atan2(poseSouthUp.x, poseSouthUp.y) * Mathf.Rad2Deg;
            poseSouthSeperated.z = Mathf.Atan2(poseSouthUp.y, poseSouthUp.z) * Mathf.Rad2Deg;

            poseEastSeperated.x = Mathf.Atan2(poseEastForward.x, poseEastForward.z) * Mathf.Rad2Deg;
            poseEastSeperated.y = Mathf.Atan2(poseEastUp.x, poseEastUp.y) * Mathf.Rad2Deg;
            poseEastSeperated.z = Mathf.Atan2(poseEastUp.y, poseEastUp.z) * Mathf.Rad2Deg;

            poseWestSeperated.x = Mathf.Atan2(poseWestForward.x, poseWestForward.z) * Mathf.Rad2Deg;
            poseWestSeperated.y = Mathf.Atan2(poseWestUp.x, poseWestUp.y) * Mathf.Rad2Deg;
            poseWestSeperated.z = Mathf.Atan2(poseWestUp.y, poseWestUp.z) * Mathf.Rad2Deg;

            deltaLiveOriginXZ = Mathf.DeltaAngle(zeroXZ, liveXZ);
            deltaLiveOriginXY = Mathf.DeltaAngle(zeroXY, liveXY);
            deltaLiveOriginYZ = Mathf.DeltaAngle(zeroYZ, liveYZ);

            NorthTotalScore = PopulateScores(poseNorthSeperated, "North");
            SouthTotalScore = PopulateScores(poseSouthSeperated, "South");
            EastTotalScore = PopulateScores(poseEastSeperated, "East");
            WestTotalScore = PopulateScores(poseWestSeperated, "West");

            /*
            DebugGraph.MultiLog("Pose y ", Color.red, live_origin_delta_y, "live_origin_delta_y");
            DebugGraph.MultiLog("Pose y ", Color.blue, pose_attach_delta_y, "pose_attach_delta_y");
            DebugGraph.MultiLog("Pose x ", Color.red, live_origin_delta_x, "live_origin_delta_x");
            DebugGraph.MultiLog("Pose x ", Color.blue, pose_attach_delta_x, "pose_attach_delta_x");
            DebugGraph.MultiLog("Pose z ", Color.red, live_origin_delta_z, "live_origin_delta_z");
            DebugGraph.MultiLog("Pose z ", Color.blue, pose_attach_delta_z, "pose_attach_delta_z");
            */

            if (!float.IsNaN(NorthTotalScore))
                North.transform.localScale = new Vector3(1f, NorthTotalScore, 1f);
            if (!float.IsNaN(SouthTotalScore))
                South.transform.localScale = new Vector3(1f, SouthTotalScore, 1f);
            if (!float.IsNaN(EastTotalScore))
                East.transform.localScale = new Vector3(1f, EastTotalScore, 1f);
            if (!float.IsNaN(WestTotalScore))
                West.transform.localScale = new Vector3(1f, WestTotalScore, 1f);
        }

        private float PopulateScores(Vector3 PoseSeperated, string Name)
        {
            float Score;

            var deltaPoseOriginXZ = Mathf.DeltaAngle(zeroXZ, PoseSeperated.x);
            var deltaPoseOriginXY = Mathf.DeltaAngle(zeroXY, PoseSeperated.y);
            var deltaPoseOriginYZ = Mathf.DeltaAngle(zeroYZ, PoseSeperated.z);

            DebugGraph.MultiLog("Pose XZ " + Name, Color.green, deltaPoseOriginXZ, "deltaPoseOriginXZ");
            DebugGraph.MultiLog("Pose XY " + Name, Color.green, deltaPoseOriginXY, "deltaPoseOriginXY");
            DebugGraph.MultiLog("Pose YZ " + Name, Color.green, deltaPoseOriginYZ, "deltaPoseOriginYZ");

            DebugGraph.MultiLog("Pose XZ " + Name, Color.blue, deltaLiveOriginXZ, "deltaLiveOriginXZ");
            DebugGraph.MultiLog("Pose XY " + Name, Color.blue, deltaLiveOriginXY, "deltaLiveOriginXY");
            DebugGraph.MultiLog("Pose YZ " + Name, Color.blue, deltaLiveOriginYZ, "deltaLiveOriginYZ");

            Vector3 ScoreSeperated = new Vector3((deltaLiveOriginXY / deltaPoseOriginXY).Clamp(0f, 1f), (deltaLiveOriginXZ / deltaPoseOriginXZ).Clamp(0f, 1f), (deltaLiveOriginYZ / deltaPoseOriginYZ).Clamp(0f, 1f));

            Score = ScoreSeperated.x + ScoreSeperated.y + ScoreSeperated.z;
            Score = Remap(Score, 0f, 3f, 0f, 1f);

            return Score;
        }

        public float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return Mathf.Clamp((value - from1) / (to1 - from1) * (to2 - from2) + from2, from2, to2);
        }

        [Button(ButtonSizes.Medium)]
        [HorizontalGroup("ButtonsNS")]
        public void SnapshotNorthPose()
        {
            poseNorth = gameObject.transform.localRotation;
        }

        [Button(ButtonSizes.Medium)]
        [HorizontalGroup("ButtonsNS")]
        public void SnapshotSouthPose()
        {
            poseSouth = gameObject.transform.localRotation;

        }

        [Button(ButtonSizes.Medium)]
        [HorizontalGroup("ButtonsEW")]
        public void SnapshotEastPose()
        {
            poseEast = gameObject.transform.localRotation;

        }

        [Button(ButtonSizes.Medium)]
        [HorizontalGroup("ButtonsEW")]
        public void SnapshotWestPose()
        {
            poseWest = gameObject.transform.localRotation;
        }

    }
}
