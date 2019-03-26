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
    }
}