using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet
{
    public class ButtPuppet : MonoBehaviour
    {
        [Required]
        public MrPuppetDataMapper DataMapper;

        private bool AttachPoseSet = false;
        private Quaternion AttachPoseElbowRotation;
        private Quaternion AttachPoseWristRotation;
        private Vector3 AttachPoseElbowPosition;

        private Vector3 BindPoseButtPosition;
        private Quaternion BindPoseButtRotation;
        private Quaternion BindPoseNeckRotation;

        public Transform Butt;
        public Transform Neck;

        [MinValue(0f)]
        public float RotationSpeed = 7f;
        [MinValue(0f)]
        public float PositionSpeed = 0.1f;
        private Vector3 PositionVelocity;

        private void OnValidate()
        {
            if (!DataMapper) DataMapper = FindObjectOfType<MrPuppetDataMapper>();
        }

        private bool ApplicationIsPlaying()
        {
            return Application.isPlaying;
        }

        [Button(ButtonSizes.Large)]
        [GUIColor(0f, 1f, 0f)]
        [EnableIf(nameof(ApplicationIsPlaying))]
        public void GrabAttachPose()
        {
            AttachPoseSet = true;

            // grab the attach position of the elbow joint
            AttachPoseElbowPosition = DataMapper.ElbowJoint.position;

            // grab the attach rotation of the joints
            AttachPoseElbowRotation = DataMapper.ElbowJoint.rotation;
            AttachPoseWristRotation = DataMapper.WristJoint.rotation;
        }

        private void Awake()
        {
            BindPoseButtPosition = Butt.position;
            BindPoseButtRotation = Butt.rotation;
            BindPoseNeckRotation = Neck.rotation;
        }

        private void Update()
        {
            if (AttachPoseSet)
            {
                // apply position delta to bind pose
                Butt.position = Vector3.SmoothDamp(Butt.position, BindPoseButtPosition + (DataMapper.ElbowJoint.position - AttachPoseElbowPosition), ref PositionVelocity, PositionSpeed);

                // apply rotation deltas to bind pose
                Butt.rotation = Quaternion.Slerp(Butt.rotation, (DataMapper.ElbowJoint.rotation * Quaternion.Inverse(AttachPoseElbowRotation)) * BindPoseButtRotation, RotationSpeed * Time.smoothDeltaTime);
                Neck.rotation = Quaternion.Slerp(Neck.rotation, (DataMapper.WristJoint.rotation * Quaternion.Inverse(AttachPoseWristRotation)) * BindPoseNeckRotation, RotationSpeed * Time.smoothDeltaTime);
            }
        }
    }
}
