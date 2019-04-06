using UnityEngine;

namespace Thinko
{
    [RequireComponent(typeof(Animator))]
    public class RealPuppet : MonoBehaviour
    {
        public RealPuppetDataProvider RealPuppetDataProvider;
        
        [Header("Joints")]
        public Transform RootNode;
        public Transform ButtNode;
        
        [Header("Head")] 
        public bool AnimateHead = true;
        public Transform HeadNode;
        public Vector3 HeadRotationOffset;
        [Range(0f, 1f)]
        public float HeadRotationSharpness = 0.5f;

        public bool DebugDrawRotation;

        [Header("Jaw")] 
        public bool AnimateJaw = true;
        public Transform JawNode;
        public Transform JawInitialPose;
        public Transform JawExtremePose;
        [Range(0, .3f)]
        public float JawSmoothness = .15f;
        public float JawMin = 0;
        public float JawMax = 1023;
        [ReadOnly]
        public float JawGlove;

        private float _jawNormalized;
        private Vector3 _jawCurrentVelocity;

        private void Reset()
        {
            // Attempt to automatically find the nodes by their names
            var children = gameObject.GetComponentsInChildren<Transform>();
            foreach(var child in children)
            {
                if (StringComparison.DamerauLevenshteinDistance(child.name.ToLower(), "skeleton") < 2 ||
                    StringComparison.DamerauLevenshteinDistance(child.name.ToLower(), "root") < 2)
                {
                    RootNode = child;
                    continue;
                }
                
                if (StringComparison.DamerauLevenshteinDistance(child.name.ToLower(), "head") < 2)
                {
                    HeadNode = child;
                    continue;
                }
                
                if (StringComparison.DamerauLevenshteinDistance(child.name.ToLower(), "butt") < 2 ||
                    StringComparison.DamerauLevenshteinDistance(child.name.ToLower(), "tail") < 2)
                {
                    ButtNode = child;
                    continue;
                }
                
                if (StringComparison.DamerauLevenshteinDistance(child.name.ToLower(), "jaw") < 2)
                {
                    JawNode = child;
                }
            }
        }
        
        private void Update()
        {
            if (RealPuppetDataProvider == null)
            {
                Debug.LogWarning("No RealPuppetDataProvider available");
                return;
            }
            
            if (AnimateHead)
            {
                HeadNode.localRotation = Quaternion.Slerp(HeadNode.localRotation, RealPuppetDataProvider.WristRotation * Quaternion.Euler(HeadRotationOffset), HeadRotationSharpness);
            }
            
            if (AnimateJaw)
            {
                JawGlove = RealPuppetDataProvider.Jaw;
                _jawNormalized = Mathf.InverseLerp(JawMin, JawMax, JawGlove);
                JawNode.position = Vector3.SmoothDamp(JawNode.position, Vector3.Lerp(JawInitialPose.position, JawExtremePose.position, _jawNormalized), ref _jawCurrentVelocity, JawSmoothness);
                JawNode.localRotation = Quaternion.Lerp(JawInitialPose.localRotation, JawExtremePose.localRotation, _jawNormalized);
            }
        }
    }
}