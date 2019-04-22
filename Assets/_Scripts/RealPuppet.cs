using System;
using System.Collections.Generic;
using UnityEngine;

namespace Thinko
{
    public class RealPuppet : MonoBehaviour
    {
        [Serializable]
        public class PuppetJoint
        {
            public bool Enabled = true;
            public RealPuppetDataProvider RealPuppetDataProvider;
            public Transform Joint;
            public RealPuppetDataProvider.Source InputSource;
            public Vector3 Offset;
            [Range(0, 1)] public float Sharpness = 1;
        }
        
        public enum PuppetJawAnimMode
        {
            BlendShape,
            Transform
        }

        [Header("Joints")] 
        public List<PuppetJoint> PuppetJoints = new List<PuppetJoint>();

        [Header("Jaw")] 
        public bool AnimateJaw;
        public RealPuppetDataProvider JawRealPuppetDataProvider;
        public PuppetJawAnimMode JawAnimMode;
        public SkinnedMeshRenderer JawMeshRenderer;
        public int JawBlendShapeIndex;
        public Transform JawNode;
        public Transform JawInitialPose;
        public Transform JawExtremePose;
        [Range(0, .3f)] public float JawSmoothness = .15f;
        public float JawMin = 0;
        public float JawMax = 1023;
        [ReadOnly] public float JawGlove;

        private float _jawNormalized;
        private Vector3 _jawCurrentVelocity;
        
        [Header("Limbs")]
        public List<DynamicBone> DynamicBones = new List<DynamicBone>();

        private void Update()
        {
            foreach (var puppetJoint in PuppetJoints)
            {
                if (!puppetJoint.Enabled || puppetJoint.RealPuppetDataProvider == null || puppetJoint.Joint == null) continue;
                puppetJoint.Joint.localRotation = Quaternion.Slerp(puppetJoint.Joint.localRotation, puppetJoint.RealPuppetDataProvider.GetInput(puppetJoint.InputSource) * Quaternion.Euler(puppetJoint.Offset), puppetJoint.Sharpness);
            }

            if (AnimateJaw && JawRealPuppetDataProvider != null)
            {
                JawGlove = JawRealPuppetDataProvider.Jaw;
                _jawNormalized = Mathf.InverseLerp(JawMin, JawMax, JawGlove);

                if (JawAnimMode == PuppetJawAnimMode.Transform)
                {
                    if(JawNode == null || JawInitialPose == null || JawExtremePose == null) return;
                    JawNode.position = Vector3.SmoothDamp(JawNode.position, Vector3.Lerp(JawInitialPose.position, JawExtremePose.position, _jawNormalized), ref _jawCurrentVelocity, JawSmoothness);
                    JawNode.localRotation = Quaternion.Lerp(JawInitialPose.localRotation, JawExtremePose.localRotation, _jawNormalized);
                }
                else
                {
                    if(JawMeshRenderer == null) return;
                    JawMeshRenderer.SetBlendShapeWeight(JawBlendShapeIndex, _jawNormalized * 100f);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if(!enabled) return;
            foreach (var puppetJoint in PuppetJoints)
            {
                if (!puppetJoint.Enabled || puppetJoint.RealPuppetDataProvider == null || puppetJoint.Joint == null) continue;
                Debug.DrawRay(puppetJoint.Joint.position, puppetJoint.RealPuppetDataProvider.GetInput(puppetJoint.InputSource) * transform.forward, Color.blue, 0f, true);
                Debug.DrawRay(puppetJoint.Joint.position, puppetJoint.RealPuppetDataProvider.GetInput(puppetJoint.InputSource) * transform.up, Color.green, 0f, true);
                Debug.DrawRay(puppetJoint.Joint.position, puppetJoint.RealPuppetDataProvider.GetInput(puppetJoint.InputSource) * transform.right, Color.red, 0f, true);
            }
        }
    }
}