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
                transform.rotation = Quaternion.Inverse(AttachPose.ElbowRotation) * DataMapper.ElbowJoint.rotation;
            }
        }
    }
}
