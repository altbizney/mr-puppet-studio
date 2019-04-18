using UnityEngine;

namespace Thinko
{
    public class BlinkDrivenKeys : MonoBehaviour
    {
        public Blink Blink;

        public DrivenKeys DrivenKeys;

        private void Start()
        {
            if (Blink == null || DrivenKeys == null)
                enabled = false;
        }

        private void Update()
        {
            DrivenKeys.Step = Blink.EyelidState;
        }
    }
}