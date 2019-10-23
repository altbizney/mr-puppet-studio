using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceUp : MonoBehaviour
{
    public float thrust = 2f;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // if (Input.GetKey(KeyCode.W))
        //     rb.AddForce(0f, thrust, 0f, ForceMode.Impulse);

        // if (Input.GetKey(KeyCode.Q))
        //     rb.AddTorque(Vector3.right * spin, ForceMode.Impulse);
        // else if (Input.GetKey(KeyCode.E))
        //     rb.AddTorque(Vector3.left * spin, ForceMode.Impulse);

        // if (Input.GetKey(KeyCode.A)) rb.AddForce(0f, thrust, thrust, ForceMode.VelocityChange);
        // else if (Input.GetKey(KeyCode.D)) rb.AddForce(0f, thrust, -thrust, ForceMode.VelocityChange);
        // else if (Input.GetKey(KeyCode.W)) rb.AddForce(thrust, thrust, 0f, ForceMode.VelocityChange);
        // else if (Input.GetKey(KeyCode.S)) rb.AddForce(-thrust, thrust, 0f, ForceMode.VelocityChange);
        // else rb.AddForce(0f, thrust, 0f, ForceMode.VelocityChange);
    }
}
