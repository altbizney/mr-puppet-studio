using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using System;

public class PosespaceDotProduct : MonoBehaviour
{
    private GameObject North;
    private GameObject South;
    private GameObject East;
    private GameObject West;

    private GameObject NorthUp;
    private GameObject SouthUp;
    private GameObject EastUp;
    private GameObject WestUp;

    private GameObject NorthRight;
    private GameObject SouthRight;
    private GameObject EastRight;
    private GameObject WestRight;

    private GameObject NorthForward;
    private GameObject SouthForward;
    private GameObject EastForward;
    private GameObject WestForward;

    private Quaternion poseNorth;
    private Quaternion poseSouth;
    private Quaternion poseEast;
    private Quaternion poseWest;

    private GameObject[] JointsArm = new GameObject[3];

    //one gameobject for each pose

    private float[] NorthDotProduct = new float[3];
    private float[] SouthDotProduct = new float[3];
    private float[] EastDotProduct = new float[3];
    private float[] WestDotProduct = new float[3];

    /*
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
    */

    private void Awake()
    {
        SnapshotNorthPose();
        SnapshotSouthPose();
        SnapshotEastPose();
        SnapshotWestPose();

        North = GameObject.Find("North");
        South = GameObject.Find("South");
        East = GameObject.Find("East");
        West = GameObject.Find("West");

        NorthForward = GameObject.Find("NorthForward");
        SouthForward = GameObject.Find("SouthForward");
        EastForward = GameObject.Find("EastForward");
        WestForward = GameObject.Find("WestForward");

        NorthUp = GameObject.Find("NorthUp");
        SouthUp = GameObject.Find("SouthUp");
        EastUp = GameObject.Find("EastUp");
        WestUp = GameObject.Find("WestUp");

        NorthRight = GameObject.Find("NorthRight");
        SouthRight = GameObject.Find("SouthRight");
        EastRight = GameObject.Find("EastRight");
        WestRight = GameObject.Find("WestRight");
    }


    void Update()
    {
        NorthDotProduct = PopulateVector(poseNorth);
        SouthDotProduct = PopulateVector(poseSouth);
        EastDotProduct = PopulateVector(poseEast);
        WestDotProduct = PopulateVector(poseWest);

        //Pillar Heights
        /*
        North.transform.localScale = new Vector3(1f, (NorthDotProduct[0] + NorthDotProduct[1] + NorthDotProduct[2]) / 3, 1f);
        South.transform.localScale = new Vector3(1f, (SouthDotProduct[0] + SouthDotProduct[1] + SouthDotProduct[2]) / 3, 1f);
        East.transform.localScale = new Vector3(1f, (EastDotProduct[0] + EastDotProduct[1] + EastDotProduct[2]) / 3, 1f);
        West.transform.localScale = new Vector3(1f, (WestDotProduct[0] + WestDotProduct[1] + WestDotProduct[2]) / 3, 1f);

        NorthForward.transform.localScale = new Vector3(1f, NorthDotProduct[0], 1f);
        SouthForward.transform.localScale = new Vector3(1f, SouthDotProduct[0], 1f);
        EastForward.transform.localScale = new Vector3(1f, EastDotProduct[0], 1f);
        WestForward.transform.localScale = new Vector3(1f, WestDotProduct[0], 1f);

        NorthUp.transform.localScale = new Vector3(1f, NorthDotProduct[1], 1f);
        SouthUp.transform.localScale = new Vector3(1f, SouthDotProduct[1], 1f);
        EastUp.transform.localScale = new Vector3(1f, EastDotProduct[1], 1f);
        WestUp.transform.localScale = new Vector3(1f, WestDotProduct[1], 1f);

        NorthRight.transform.localScale = new Vector3(1f, NorthDotProduct[2], 1f);
        SouthRight.transform.localScale = new Vector3(1f, SouthDotProduct[2], 1f);
        EastRight.transform.localScale = new Vector3(1f, EastDotProduct[2], 1f);
        WestRight.transform.localScale = new Vector3(1f, WestDotProduct[2], 1f);
        */
    }


    private float[] PopulateVector(Quaternion Pose)
    {
        float[] DotProductValues = new float[3];

        DotProductValues[0] = Vector3.Dot(transform.forward, Pose * Vector3.forward);
        DotProductValues[1] = Vector3.Dot(transform.up, Pose * Vector3.up);
        DotProductValues[2] = Vector3.Dot(transform.right, Pose * Vector3.right);

        DotProductValues[0] = Remap(DotProductValues[0], -1f, 1f, 0f, 1f);
        DotProductValues[1] = Remap(DotProductValues[1], -1f, 1f, 0f, 1f);
        DotProductValues[2] = Remap(DotProductValues[2], -1f, 1f, 0f, 1f);

        return DotProductValues;
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
