using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]

public class HoverBone : MonoBehaviour
{

    [Header("Hover Settings")]
    public float hoverHeight = 1f;
    public LayerMask hoverOnLayer;
    public float hoverForce = 5f;
    public bool isHovering = true;
    public Rigidbody rb;
    public LocalForceDirection localHoverForceDirection = LocalForceDirection.Down;

    Vector3 hoverDirection;
    //public float height = 1f;

    Vector3 forceDirection;



    // Use this for initialization
    void Start()
    {

        rb = GetComponent<Rigidbody>();
        SetHoverForceDirection();
        //SetHoverHeight();

    }


    void SetHoverForceDirection() { 
    
        if (localHoverForceDirection == LocalForceDirection.Up){
            hoverDirection = rb.transform.up;
        }
        if (localHoverForceDirection == LocalForceDirection.Down)
        {
            hoverDirection = -rb.transform.up;
        }
        if (localHoverForceDirection == LocalForceDirection.Left)
        {
            hoverDirection = -rb.transform.right;
        }
        if (localHoverForceDirection == LocalForceDirection.Right)
        {
            hoverDirection = rb.transform.right;
        }
        if (localHoverForceDirection == LocalForceDirection.Forward)
        {
            hoverDirection = rb.transform.forward;
        }
        if (localHoverForceDirection == LocalForceDirection.Backward)
        {
            hoverDirection = -rb.transform.forward;
        }



    }

    void SetHoverHeight()
    {

        RaycastHit hit;

        if (Physics.Raycast(rb.transform.position, -rb.transform.up.normalized * 100f, out hit))
        {
            hoverHeight = hit.distance;
        }

    }

    private void FixedUpdate()
    {
    
        if (isHovering)
        {

            Ray hoverRay = new Ray(rb.transform.position, hoverDirection);
            RaycastHit hoverRayHit;

            Debug.DrawRay(rb.transform.position, hoverDirection, Color.green);

            if (Physics.Raycast(hoverRay, out hoverRayHit, hoverHeight, hoverOnLayer))
            {

                Debug.DrawRay(rb.transform.position, hoverDirection, Color.blue);

                float proportionalHeight = (hoverHeight - hoverRayHit.distance) / hoverHeight;
                Vector3 appliedHoverForce = -hoverDirection * proportionalHeight * hoverForce;
                rb.AddForce(appliedHoverForce, ForceMode.Acceleration);
//                Debug.Log(appliedHoverForce);

            } // Raycast hit


        } // (isStanding)


        //Vector3 forceDirection = hoverRayHit.normal;



        //Debug.DrawRay(rb.transform.position, forceDirection * 2f, Color.red);
        //Debug.Log(forceDirection);


    }

}
public enum LocalForceDirection { Up, Down, Left, Right, Forward, Backward }