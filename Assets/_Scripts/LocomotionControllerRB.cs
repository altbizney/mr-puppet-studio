using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet.WIP
{
    public class LocomotionControllerRB : MonoBehaviour
    {
        public float speed = 10.0f;
        public float gravity = 10.0f;
        public float maxVelocityChange = 10.0f;
        public bool canJump = true;
        public float jumpHeight = 2.0f;

        private Rigidbody rb;
        private bool grounded = true;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
            rb.useGravity = false;
        }

        private void FixedUpdate()
        {
            if (grounded)
            {
                // Calculate how fast we should be moving
                Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
                targetVelocity = transform.TransformDirection(targetVelocity);
                targetVelocity *= speed;

                // Apply a force that attempts to reach our target velocity
                Vector3 velocity = rb.velocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0f;
                rb.AddForce(velocityChange, ForceMode.VelocityChange);

                // Jump
                if (canJump && Input.GetKey(KeyCode.Space))
                {
                    rb.velocity = new Vector3(velocity.x, CalculateJumpVerticalSpeed(), velocity.z);
                }

                // We apply gravity manually for more tuning control
                rb.AddForce(new Vector3(0f, -gravity * rb.mass, 0f));
            }
        }

        private void OnCollisionStay()
        {
            grounded = true;
        }

        private float CalculateJumpVerticalSpeed()
        {
            // From the jump height and gravity we deduce the upwards speed
            // for the character to reach at the apex.
            return Mathf.Sqrt(2f * jumpHeight * gravity);
        }

    }
}
