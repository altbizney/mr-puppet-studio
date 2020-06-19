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

            liveXZ = Mathf.Atan2(Mathf.Sin(liveForward.x), Mathf.Cos(liveForward.z)) * Mathf.Rad2Deg;
            liveXY = Mathf.Atan2(Mathf.Sin(liveUp.x), Mathf.Cos(liveUp.y)) * Mathf.Rad2Deg;
            liveYZ = Mathf.Atan2(Mathf.Sin(liveForward.y), Mathf.Cos(liveForward.z)) * Mathf.Rad2Deg;

            //DebugGraph.MultiLog("Atan ", Color.white, Mathf.Atan2(liveForward.x, liveForward.)z) * Mathf.Rad2Deg, "Atan");
            //DebugGraph.MultiLog("Atan", Color.blue, Mathf.Atan2(Mathf.Sin(liveForward.x), Mathf.Cos(liveForward.z)) * Mathf.Rad2Deg, "Atan");

            zeroXZ = Mathf.Atan2(Mathf.Sin(zeroForward.y), Mathf.Cos(zeroForward.z)) * Mathf.Rad2Deg;
            zeroXY = Mathf.Atan2(Mathf.Sin(zeroUp.x), Mathf.Cos(zeroUp.y)) * Mathf.Rad2Deg;
            zeroYZ = Mathf.Atan2(Mathf.Sin(zeroForward.y), Mathf.Cos(zeroForward.z)) * Mathf.Rad2Deg;

            poseNorthSeperated.x = Mathf.Atan2(Mathf.Sin(poseNorthForward.x), Mathf.Cos(poseNorthForward.z)) * Mathf.Rad2Deg;
            poseNorthSeperated.y = Mathf.Atan2(Mathf.Sin(poseNorthUp.x), Mathf.Cos(poseNorthUp.y)) * Mathf.Rad2Deg;
            poseNorthSeperated.z = Mathf.Atan2(Mathf.Sin(poseNorthForward.y), Mathf.Cos(poseNorthForward.z)) * Mathf.Rad2Deg;

            poseSouthSeperated.x = Mathf.Atan2(Mathf.Sin(poseSouthForward.x), Mathf.Cos(poseSouthForward.z)) * Mathf.Rad2Deg;
            poseSouthSeperated.y = Mathf.Atan2(Mathf.Sin(poseSouthUp.x), Mathf.Cos(poseSouthUp.y)) * Mathf.Rad2Deg;
            poseSouthSeperated.z = Mathf.Atan2(Mathf.Sin(poseSouthForward.y), Mathf.Cos(poseSouthForward.z)) * Mathf.Rad2Deg;

            poseEastSeperated.x = Mathf.Atan2(Mathf.Sin(poseEastForward.x), Mathf.Cos(poseEastForward.z)) * Mathf.Rad2Deg;
            poseEastSeperated.y = Mathf.Atan2(Mathf.Sin(poseEastUp.x), Mathf.Cos(poseEastUp.y)) * Mathf.Rad2Deg;
            poseEastSeperated.z = Mathf.Atan2(Mathf.Sin(poseEastForward.y), Mathf.Cos(poseEastForward.z)) * Mathf.Rad2Deg;

            poseWestSeperated.x = Mathf.Atan2(Mathf.Sin(poseWestForward.x), Mathf.Cos(poseWestForward.z)) * Mathf.Rad2Deg;
            poseWestSeperated.y = Mathf.Atan2(Mathf.Sin(poseWestUp.x), Mathf.Cos(poseWestUp.y)) * Mathf.Rad2Deg;
            poseWestSeperated.z = Mathf.Atan2(Mathf.Sin(poseWestForward.y), Mathf.Cos(poseWestForward.z)) * Mathf.Rad2Deg;

            deltaLiveOriginXZ = Mathf.DeltaAngle(zeroXZ, liveXZ);
            deltaLiveOriginXY = Mathf.DeltaAngle(zeroXY, liveXY);
            deltaLiveOriginYZ = Mathf.DeltaAngle(zeroYZ, liveYZ);

            NorthTotalScore = PopulateScores(poseNorthSeperated, "North");
            SouthTotalScore = PopulateScores(poseSouthSeperated, "South");
            EastTotalScore = PopulateScores(poseEastSeperated, "East");
            WestTotalScore = PopulateScores(poseWestSeperated, "West");

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

            DebugGraph.MultiLog("Pose XZ " + Name, Color.blue, EulerAnglesClamp(deltaLiveOriginXZ), "deltaLiveOriginXZ");
            DebugGraph.MultiLog("Pose XY " + Name, Color.blue, EulerAnglesClamp(deltaLiveOriginXY), "deltaLiveOriginXY");
            DebugGraph.MultiLog("Pose YZ " + Name, Color.blue, EulerAnglesClamp(deltaLiveOriginYZ), "deltaLiveOriginYZ");

            DebugGraph.MultiLog("Pose XZ " + Name, Color.white, deltaLiveOriginXZ, "deltaLiveOriginXZ");
            DebugGraph.MultiLog("Pose XY " + Name, Color.white, deltaLiveOriginXY, "deltaLiveOriginXY");
            DebugGraph.MultiLog("Pose YZ " + Name, Color.white, deltaLiveOriginYZ, "deltaLiveOriginYZ");

            Vector3 ScoreSeperated = new Vector3((deltaLiveOriginXY / deltaPoseOriginXY).Clamp(0f, 1f), (deltaLiveOriginXZ / deltaPoseOriginXZ).Clamp(0f, 1f), (deltaLiveOriginYZ / deltaPoseOriginYZ).Clamp(0f, 1f));
            Score = ScoreSeperated.x + ScoreSeperated.y + ScoreSeperated.z;
            Score = Remap(Score, 0f, 3f, 0f, 1f);

            return Score;
        }

        private float EulerAnglesClamp(float value)
        {
            //Vector3 delta = from.eulerAngles;
            float newValue = value;

            if (newValue > 180)
                newValue -= 360;
            else if (newValue < -180)
                newValue += 360;

            return newValue;
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
