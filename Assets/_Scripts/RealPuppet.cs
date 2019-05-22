using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Thinko
{
    public class RealPuppet : MonoBehaviour
    {
        public enum PuppetJawAnimMode
        {
            BlendShape,
            Transform
        }
        
        [Required]
        public RealBody RealBody;

        public Transform ShoulderJoint;
        public Transform ElbowJoint;
        public Transform WristJoint;

        [Header("Jaw")] public bool AnimateJaw;
        public RealPuppetDataProvider JawRealPuppetDataProvider;
        public PuppetJawAnimMode JawAnimMode;
        public SkinnedMeshRenderer JawMeshRenderer;
        public int JawBlendShapeIndex;
        public Transform JawNode;
        public float JawInitialPose = 0f;
        public float JawExtremePose = 50f;
        [Range(0, .3f)] public float JawSmoothness = .15f;
        public float JawMin = 0;
        public float JawMax = 1023;
        [ReadOnly] public float JawGlove;

        private float _jawNormalized;
        private float _jawNormalizedSmoothed;
        private Vector3 _jawCurrentVelocity;
        private float _jawCurrentVelocityF;
        private float _jawSmoothed;

        [Header("Limbs")]
        public List<DynamicBone> DynamicBones = new List<DynamicBone>();

        private void Update()
        {
            // Joints
            if (ShoulderJoint)
                ShoulderJoint.localRotation = RealBody.FinalPose.ShoulderRotation;
                
            if (ElbowJoint)
                ElbowJoint.localRotation = RealBody.FinalPose.ElbowRotation;
                
            if (WristJoint)
                WristJoint.localRotation = RealBody.FinalPose.WristRotation;
            
            // Jaw
            if (AnimateJaw && JawRealPuppetDataProvider != null)
            {
                JawGlove = JawRealPuppetDataProvider.Jaw;
                _jawNormalized = Mathf.InverseLerp(JawMin, JawMax, JawGlove);
                _jawNormalizedSmoothed = Mathf.SmoothDamp(_jawNormalizedSmoothed, _jawNormalized, ref _jawCurrentVelocityF, JawSmoothness);

                if (JawAnimMode == PuppetJawAnimMode.Transform)
                {
                    if (JawNode == null) return;
                    // JawNode.position = Vector3.SmoothDamp(JawNode.position, Vector3.Lerp(JawInitialPose.position, JawExtremePose.position, _jawNormalized), ref _jawCurrentVelocity, JawSmoothness);
                    JawNode.localRotation = Quaternion.Lerp(Quaternion.Euler(0, 0, JawInitialPose), Quaternion.Euler(0, 0, JawExtremePose), _jawNormalizedSmoothed);
                }
                else
                {
                    if (JawMeshRenderer == null) return;
                    _jawSmoothed = Mathf.SmoothDamp(_jawSmoothed, _jawNormalized * 100f, ref _jawCurrentVelocityF, JawSmoothness);
                    JawMeshRenderer.SetBlendShapeWeight(JawBlendShapeIndex, _jawSmoothed);
                }
            }
        }
    }
}