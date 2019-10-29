using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MrPuppet
{
    public class ForceController : MonoBehaviour
    {
        public Rigidbody rb;
        public float WalkSpeed = 5f;

        public float BobAmplitude = 1f;
        public float BobSpeed = 2f;

        private Vector3 InputVelocity;

        private float Bob = 0f;
        private float BobStartTime = 0f;
        private float BobWave = 0f;

        private void Update()
        {
            if (Input.GetAxis("Horizontal") != 0f)
            {
                if (BobStartTime == 0f) BobStartTime = Time.time;

                // moving
                BobWave = Mathf.Sin((Time.time - BobStartTime) * BobSpeed);
                Bob = Mathf.LerpUnclamped(0f, BobAmplitude, Input.GetAxis("Horizontal")) * BobWave;
            }
            else
            {
                // resting
                BobWave = BobStartTime = 0f;
            }

            InputVelocity = new Vector3(Input.GetAxis("Horizontal") * WalkSpeed, Input.GetAxis("Horizontal") == 0f ? rb.velocity.y : Bob, rb.velocity.z);

            DebugGraph.Log("BobWave", BobWave);
            DebugGraph.Log("Bob", Bob);
            DebugGraph.Log("InputVelocity", InputVelocity);
        }

        private void FixedUpdate()
        {
            rb.velocity = InputVelocity;
        }
    }
}