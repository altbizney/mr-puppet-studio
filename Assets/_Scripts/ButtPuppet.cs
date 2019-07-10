using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet
{
    public class ButtPuppet : MonoBehaviour
    {
        [Required]
        public MrPuppetDataMapper DataMapper;

        private Pose AttachPose;
        private Vector3 AttachPosePositionElbow;

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
            AttachPosePositionElbow = DataMapper.ElbowJoint.position;

            AttachPose = new Pose
            {
                ShoulderRotation = DataMapper.ShoulderJoint.rotation,
                ElbowRotation = DataMapper.ElbowJoint.rotation,
                WristRotation = DataMapper.WristJoint.rotation
            };
        }

        private void Update()
        {
            if (AttachPose != null)
            {
                Butt.position = DataMapper.ElbowJoint.position - AttachPosePositionElbow;
                Butt.rotation = DataMapper.ElbowJoint.rotation * Quaternion.Inverse(AttachPose.ElbowRotation);
                Neck.rotation = DataMapper.WristJoint.rotation * Quaternion.Inverse(AttachPose.WristRotation);
            }
        }
    }
}
