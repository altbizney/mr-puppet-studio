using UnityEngine;

namespace Thinko
{
    [RequireComponent(typeof(Animation))]
    public class BlinkAnimator : Blink
    {
        private AnimationState _animState;

        private void Awake()
        {
            var anim = GetComponent<Animation>();
            foreach (AnimationState state in anim)
            {
                _animState = state;
            }
        }

        protected override void Update()
        {
            base.Update();
            
            _animState.normalizedTime = EyelidState.Remap(1, 0, 0, 1);
        }
    }
}