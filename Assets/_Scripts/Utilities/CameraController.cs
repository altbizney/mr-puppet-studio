using Thinko;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float Distance = 5;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            var realBody = FindObjectOfType<RealBody>();

            transform.position = realBody.WristJoint.position - realBody.WristJoint.forward * Distance;
            transform.LookAt(realBody.WristJoint.position);
        }
    }
}