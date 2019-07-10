using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet
{
    public class ButtPuppet : MonoBehaviour
    {
        [Required]
        public MrPuppetDataMapper DataMapper;

        private Vector3 AttachPositionShoulder;
        private Pose AttachPose;

        public Transform Hip;
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
            AttachPositionShoulder = DataMapper.ShoulderJoint.position;

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
                Hip.position = DataMapper.ElbowJoint.position - AttachPositionShoulder;
                Butt.rotation = DataMapper.ElbowJoint.rotation * Quaternion.Inverse(AttachPose.ElbowRotation);
                Neck.rotation = DataMapper.WristJoint.rotation * Quaternion.Inverse(AttachPose.WristRotation);
            }
        }
    }
}
