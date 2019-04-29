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
            [Range(0, 1)] public float Sharpness = .5f;
        }

        public enum PuppetJawAnimMode
        {
            BlendShape,
            Transform
        }

        public bool AutoCreateSpline = true;

        [Header("Joints")] 
        public List<PuppetJoint> PuppetJoints = new List<PuppetJoint>();

        [Header("Jaw")] public bool AnimateJaw;
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
        private float _jawNormalizedSmoothed;
        private Vector3 _jawCurrentVelocity;
        private float _jawCurrentVelocityF;
        private float _jawSmoothed;

        [Header("Limbs")] 
        public List<DynamicBone> DynamicBones = new List<DynamicBone>();

        private void Start()
        {
            // Automatically create and configure the spline component
            if (AutoCreateSpline && PuppetJoints.Count > 1)
            {
                // Sort joints by "depth"
                var orderedJoints = new List<PuppetJoint>(PuppetJoints);
                orderedJoints.Sort((x, y) => GetChildDepth(x.Joint));

                var spline = gameObject.AddComponent<RealPuppetSpline>();
                spline.RootJoint = orderedJoints[0].Joint;
                for (var i = 1; i < orderedJoints.Count; i++)
                {
                    spline.Joints.Add(orderedJoints[i].Joint);
                }
            }
        }

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
                _jawNormalizedSmoothed = Mathf.SmoothDamp(_jawNormalizedSmoothed, _jawNormalized, ref _jawCurrentVelocityF, JawSmoothness);

                if (JawAnimMode == PuppetJawAnimMode.Transform)
                {
                    if (JawNode == null || JawInitialPose == null || JawExtremePose == null) return;
                    JawNode.position = Vector3.SmoothDamp(JawNode.position, Vector3.Lerp(JawInitialPose.position, JawExtremePose.position, _jawNormalized), ref _jawCurrentVelocity, JawSmoothness);
                    JawNode.localRotation = Quaternion.Lerp(JawInitialPose.localRotation, JawExtremePose.localRotation, _jawNormalizedSmoothed);
                }
                else
                {
                    if (JawMeshRenderer == null) return;
                    _jawSmoothed = Mathf.SmoothDamp(_jawSmoothed, _jawNormalized * 100f, ref _jawCurrentVelocityF, JawSmoothness);
                    JawMeshRenderer.SetBlendShapeWeight(JawBlendShapeIndex, _jawSmoothed);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!enabled) return;
            foreach (var puppetJoint in PuppetJoints)
            {
                if (!puppetJoint.Enabled || puppetJoint.RealPuppetDataProvider == null || puppetJoint.Joint == null) continue;
                Debug.DrawRay(puppetJoint.Joint.position, puppetJoint.RealPuppetDataProvider.GetInput(puppetJoint.InputSource) * transform.forward, Color.blue, 0f, true);
                Debug.DrawRay(puppetJoint.Joint.position, puppetJoint.RealPuppetDataProvider.GetInput(puppetJoint.InputSource) * transform.up, Color.green, 0f, true);
                Debug.DrawRay(puppetJoint.Joint.position, puppetJoint.RealPuppetDataProvider.GetInput(puppetJoint.InputSource) * transform.right, Color.red, 0f, true);
            }
        }

        private static int GetChildDepth(Transform transf)
        {
            var count = 0;

            var t = transf;
            while (t.parent != null)
            {
                count++;
                t = t.parent;
            }

            return count;
        }
    }
}