using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyFollowRotation : MonoBehaviour {

    public bool followRotation;
    public Transform rotationTarget;
    public float torqueForceMultiplier = 1f;

    private Rigidbody rb;
    private Quaternion quat0;
    private Quaternion quat1;
    private Quaternion quat10;

    // Use this for initialization
    void Start()
    {

        rb = GetComponent<Rigidbody>();
    }

        // Update is called once per frame
        void FixedUpdate() {

            if (followRotation) {

                quat0 = transform.rotation;
                quat1 = rotationTarget.rotation;
                quat10 = quat1 * Quaternion.Inverse(quat0);
                rb.AddTorque(quat10.x * torqueForceMultiplier, quat10.y * torqueForceMultiplier, quat10.z * torqueForceMultiplier, ForceMode.Force);

            }
        }
    }