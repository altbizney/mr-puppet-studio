using Thinko;
using UnityEngine;

namespace Thinko
{
    public class BlinkBlendshape : Blink
    {
        public SkinnedMeshRenderer SkinnedMeshRenderer;

        public int BlendShapeIndex = 0;

        public float BlendShapeMin = 0;
        public float BlendShapeMax = 100;

        private void Start()
        {
            if (SkinnedMeshRenderer == null)
            {
                Debug.LogWarning($"{name}:{GetType()} - No SkinnedMeshRenderer defined");
                enabled = false;
            }
        }

        protected override void Update()
        {
            base.Update();
            
            SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeIndex, EyelidState.Remap(0, 1, BlendShapeMin, BlendShapeMax));
        }
    }
}