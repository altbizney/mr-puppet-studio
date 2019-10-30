using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MrPuppet
{
    public class ForceController : MonoBehaviour
    {
        public Rigidbody rb;
        public float WalkSpeed = 3.41f;

        public float BobFrequency = 0.2f;
        public float BobHeight = 4f;

        private Vector3 InputVelocity;
        private bool IsWalking = false;

        private float BobTarget = 0f;

        private void Update()
        {
            InputVelocity = rb.velocity;
            InputVelocity.x = Input.GetAxis("Horizontal") * WalkSpeed;

            IsWalking = Input.GetAxisRaw("Horizontal") != 0f;

            if (IsWalking)
            {
                if (!IsInvoking("ToggleBob")) InvokeRepeating("ToggleBob", 0f, BobFrequency);
                InputVelocity.y = BobTarget * Mathf.Lerp(0f, BobHeight, Mathf.Abs(Input.GetAxis("Horizontal")));
            }
            else
            {
                CancelInvoke("ToggleBob");
                if (BobTarget != 0f) InputVelocity.y = BobTarget = 0f;
            }

            // DebugGraph.Log("Input", Input.GetAxis("Horizontal"));
            // DebugGraph.Log("Input (raw)", Input.GetAxisRaw("Horizontal"));
            // DebugGraph.Log("BobTarget", BobTarget);
            // DebugGraph.Log("InputVelocity.x", InputVelocity.x);
            // DebugGraph.Log("InputVelocity.y", InputVelocity.y);
        }

        private void FixedUpdate()
        {
            rb.velocity = InputVelocity;
        }

        private void ToggleBob()
        {
            if (BobTarget == 1f)
            {
                BobTarget = -1f;
            }
            else
            {
                BobTarget = 1f;
            }
        }
    }
}