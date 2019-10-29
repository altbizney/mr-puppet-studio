using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceController : MonoBehaviour
{
    public float WalkSpeed = 25f;
    public float BobSpeed = 1f;

    public float MaxSpeed = 20f;

    public Rigidbody rb;

    public KeyCode LeftKey = KeyCode.LeftArrow;
    public KeyCode RightKey = KeyCode.RightArrow;
    public KeyCode UpKey = KeyCode.UpArrow;

    public ForceMode WalkMode = ForceMode.Force;
    public ForceMode BobMode = ForceMode.Impulse;

    void FixedUpdate()
    {
        if (Input.GetKey(LeftKey))
        {
            rb.AddForce(-WalkSpeed, 0f, 0f, WalkMode);
        }

        if (Input.GetKey(RightKey))
        {
            rb.AddForce(WalkSpeed, 0f, 0f, WalkMode);
        }

        if (Input.GetKey(UpKey))
        {
            rb.AddForce(0f, BobSpeed, 0f, BobMode);
        }

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, MaxSpeed);
    }
}
