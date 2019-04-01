using UnityEngine;

namespace Thinko
{
    [RequireComponent(typeof(Animator))]
    public class RealPuppet : MonoBehaviour
    {
        [Header("Joints")]
        public Transform RootNode;
        public Transform HeadNode;
        public Transform ButtNode;
        public Transform JawNode;

        [Header("Physics")]
        [Range(0, 100)]
        public float SpineFloppiness;
    
        [Header("Jaw Configuration")]
        public float GloveOpen;
        public float GloveClose;
        [ReadOnly]
        public float LiveGloveValue;

        [Space]
        public float RigOpen;
        public float RigClose;

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
    }
}