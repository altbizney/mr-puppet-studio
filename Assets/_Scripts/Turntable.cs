using System.Collections;
using UnityEngine;

namespace MrPuppet
{
    public class Turntable : MonoBehaviour
    {
        public float SmallAmount = 0.5f;
        public float BigAmount = 1f;
        public float AutospinAmount = 0f;

        private void Update()
        {
            if (AutospinAmount != 0f)
            {
                transform.Rotate(0f, AutospinAmount * Time.deltaTime, 0f);
            }

            if (Input.GetKey("left"))
            {
                transform.Rotate(0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? BigAmount : SmallAmount, 0f);
            }

            if (Input.GetKey("right"))
            {
                transform.Rotate(0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -BigAmount : -SmallAmount, 0f);
            }
        }
    }
}
