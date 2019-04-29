using UnityEngine;

namespace Thinko
{
    public class BlinkDrivenKeys : Blink
    {
        public DrivenKeys DrivenKeys;

        private void Start()
        {
            if (DrivenKeys == null)
            {
                Debug.LogWarning($"{name}:{GetType()} - No DrivenKeys defined");
                enabled = false;
            }
        }

        private void Update()
        {
            DrivenKeys.Step = EyelidState;
        }
    }
}