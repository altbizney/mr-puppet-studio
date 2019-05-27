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
        
        [Serializable]
        public class PuppetJawAnimData
        {
            public Vector3 OpenPosition = Vector3.zero;
            public Quaternion OpenRotation = Quaternion.identity;
            public Vector3 ClosePosition = Vector3.zero;
            public Quaternion CloseRotation = Quaternion.identity;
        }
        
        [Required]
        public RealBody RealBody;

        public Transform ShoulderJoint;
        public Transform ElbowJoint;
        public Transform WristJoint;

        public Vector3 ShoulderOffset;
        public Vector3 ElbowOffset;
        public Vector3 WristOffset;

        [Header("Jaw")] public bool AnimateJaw;
        public RealPuppetDataProvider JawRealPuppetDataProvider;
        public PuppetJawAnimMode JawAnimMode;
        public SkinnedMeshRenderer JawMeshRenderer;
        public int JawBlendShapeIndex;
        public Transform JawNode;
        public PuppetJawAnimData JawAnimData;
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
                ShoulderJoint.localRotation = RealBody.FinalPose.ShoulderRotation * Quaternion.Euler(ShoulderOffset);
                
            if (ElbowJoint)
                ElbowJoint.localRotation = RealBody.FinalPose.ElbowRotation * Quaternion.Euler(ElbowOffset);
                
            if (WristJoint)
                WristJoint.rotation = RealBody.FinalPose.WristRotation * Quaternion.Euler(WristOffset);
            
            // Jaw
            if (AnimateJaw && JawRealPuppetDataProvider != null)
            {
                JawGlove = JawRealPuppetDataProvider.Jaw;
                _jawNormalized = Mathf.InverseLerp(JawMin, JawMax, JawGlove);
                _jawNormalizedSmoothed = Mathf.SmoothDamp(_jawNormalizedSmoothed, _jawNormalized, ref _jawCurrentVelocityF, JawSmoothness);

                if (JawAnimMode == PuppetJawAnimMode.Transform)
                {
                    if (JawNode == null) return;
                    JawNode.localPosition = Vector3.SmoothDamp(JawNode.localPosition, Vector3.Lerp(JawAnimData.ClosePosition, JawAnimData.OpenPosition, _jawNormalized), ref _jawCurrentVelocity, JawSmoothness);
                    JawNode.localRotation = Quaternion.Lerp(JawAnimData.CloseRotation, JawAnimData.OpenRotation, _jawNormalizedSmoothed);
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