using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


namespace MrPuppet
{
    public class PoseSpaceTest : MonoBehaviour
    {
        private MrPuppetDataMapper DataMapper;
        private Quaternion ElbowRotation;
        private Quaternion ShoulderRotation;
        private Quaternion WristRotation;


        [ReadOnly]
        public Vector3 ElbowAxis;
        [ReadOnly]
        public float ElbowAngle;
        [ReadOnly]
        public Vector3 ElbowEuler;

        [ReadOnly]
        public Vector3 ShoulderAxis;
        [ReadOnly]
        public float ShoulderAngle;
        [ReadOnly]
        public Vector3 ShoulderEuler;

        [ReadOnly]
        public Vector3 WristAxis;
        [ReadOnly]
        public float WristAngle;
        [ReadOnly]
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

            //Debug.Log(yRotation(ElbowRotation).eulerAngles.y);
        }

        private Quaternion yRotation(Quaternion q)
        {
            float theta = Mathf.Atan2(q.y, q.w);

            return new Quaternion(0, Mathf.Sin(theta), 0, Mathf.Cos(theta));
        }
    }
}