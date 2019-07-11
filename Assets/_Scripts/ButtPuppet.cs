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

        private void OnValidate()
        {
            if (!DataMapper) DataMapper = FindObjectOfType<MrPuppetDataMapper>();
        }

        private bool CaptureButtonsEnabled()
        {
            return !Application.isPlaying;
        }

        [Button(ButtonSizes.Large)]
        [GUIColor(0f, 1f, 0f)]
        [DisableIf("CaptureButtonsEnabled")]
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
                Butt.position = BindPoseButtPosition + (DataMapper.ElbowJoint.position - AttachPoseElbowPosition);

                // apply rotation deltas to bind pose
                Butt.rotation = (DataMapper.ElbowJoint.rotation * Quaternion.Inverse(AttachPoseElbowRotation)) * BindPoseButtRotation;
                Neck.rotation = (DataMapper.WristJoint.rotation * Quaternion.Inverse(AttachPoseWristRotation)) * BindPoseNeckRotation;
            }
        }
    }
}
