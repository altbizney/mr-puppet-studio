using System.Collections;
using UnityEngine;

namespace MrPuppet {
    public class Turntable : MonoBehaviour {
        private float SmallAmount = 0.5f;
        private float BigAmount = 1f;

        private void Update() {
            if (Input.GetKey("left")) {
                transform.Rotate(0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? BigAmount : SmallAmount, 0f);
            }

            if (Input.GetKey("right")) {
                transform.Rotate(0f, (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? -BigAmount : -SmallAmount, 0f);
            }
        }
    }
}
