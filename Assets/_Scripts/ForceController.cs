using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceController : MonoBehaviour
{
    public Rigidbody rb;
    public float WalkSpeed = 5f;

    public float BobAmplitude = 1f;
    public float BobSpeed = 2f;

    private Vector3 InputVelocity;

    private void Update()
    {
        // Mathf.Lerp(0f, BobAmplitude, Input.GetAxis("Horizontal"));

        float Bob = (Mathf.Lerp(0f, BobAmplitude, Input.GetAxis("Horizontal")) * Mathf.Sin(Time.time * BobSpeed));

        InputVelocity = new Vector3(Input.GetAxis("Horizontal") * WalkSpeed, Input.GetAxis("Horizontal") == 0f ? rb.velocity.y : Bob, rb.velocity.z);
        // rb.transform.LookAt(new Vector3(InputVelocity.x, 0f, InputVelocity.z));
    }

    private void FixedUpdate()
    {
        rb.velocity = InputVelocity;
    }
}
