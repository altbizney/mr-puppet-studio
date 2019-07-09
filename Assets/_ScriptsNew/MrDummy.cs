using Sirenix.OdinInspector;
using UnityEngine;

namespace Thinko.MrPuppet
{
    [ExecuteInEditMode]
    public class MrDummy : MonoBehaviour
    {
        [Required]
        public MrPuppetDataMapper DataMapper;
        [Required]
        public Transform ShoulderJoint;
        [Required]
        public Transform ShoulderGeo;
        [Required]
        public Transform ElbowJoint;
        [Required]
        public Transform ElbowGeo;
        [Required]
        public Transform WristJoint;
        [Required]
        public Transform JawJoint;
        
        private void Reset()
        {
            DataMapper = FindObjectOfType<MrPuppetDataMapper>();
        }
        
        private void Update()
        {
            // Apply arm length
            var armLength = ShoulderGeo.localScale;
            armLength.x = DataMapper.ArmLength;
            ShoulderGeo.localScale = armLength;
            
            var forearmLength = ElbowGeo.localScale;
            forearmLength.x = DataMapper.ForearmLength;
            ElbowGeo.localScale = forearmLength;
            
            // Adjust joints positions
            var elbowPos = ElbowJoint.localPosition;
            elbowPos.x = DataMapper.ArmLength;
            ElbowJoint.localPosition = elbowPos;

            var wristPos = WristJoint.localPosition;
            wristPos.x = DataMapper.ForearmLength;
            WristJoint.localPosition = wristPos;
            
            // We don't want to move the joints and jaw at edit time
            if(!Application.isPlaying) return;
            
            // Rotate the joints
            ShoulderJoint.rotation = DataMapper.FinalPose.ShoulderRotation;
            ElbowJoint.rotation = DataMapper.FinalPose.ElbowRotation;
            WristJoint.rotation = DataMapper.FinalPose.WristRotation;
            
            // Apply jaw movement
            JawJoint.localEulerAngles = Vector3.forward * Mathf.LerpAngle(0f, -45.0f, DataMapper.JawPercent);
        }
    }
}