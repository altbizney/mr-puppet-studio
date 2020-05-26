using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MrPuppet
{
    public class PoseSpaceTest : MonoBehaviour
    {
        MrPuppetDataMapper DataMapper;
        Quaternion ElbowRotation;
        Quaternion ShoulderRotation;
        Quaternion WristRotation;

        public Vector3 ElbowAxis;
        public float ElbowAngle;
        public Vector3 ElbowEuler;

        public Vector3 ShoulderAxis;
        public float ShoulderAngle;
        public Vector3 ShoulderEuler;

        public Vector3 WristAxis;
        public float WristAngle;
        public Vector3 WristEuler;

        void Awake()
        {
            DataMapper = FindObjectOfType<MrPuppetDataMapper>();
        }

        void Update()
        {
            ElbowRotation = DataMapper.ElbowJoint.rotation * Quaternion.Inverse(DataMapper.AttachPose.ElbowRotation);
            WristRotation = DataMapper.WristJoint.rotation * Quaternion.Inverse(DataMapper.AttachPose.WristRotation);
            ShoulderRotation = DataMapper.ShoulderJoint.rotation * Quaternion.Inverse(DataMapper.AttachPose.ShoulderRotation);

            ElbowRotation.ToAngleAxis(out ElbowAngle, out ElbowAxis);
            WristRotation.ToAngleAxis(out WristAngle, out WristAxis);
            ShoulderRotation.ToAngleAxis(out ShoulderAngle, out ShoulderAxis);

            ElbowEuler = ElbowRotation.eulerAngles;
            WristEuler = WristRotation.eulerAngles;
            ShoulderEuler = ShoulderRotation.eulerAngles;

        }
    }
}