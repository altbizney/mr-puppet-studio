using System;
using UnityEngine;

public class ClampTest : MonoBehaviour
{
    public float minX;
    public float maxX;

    public float minY;
    public float maxY;

    public float minZ;
    public float maxZ;

    public float outputX;
    public float outputY;
    public float outputZ;

    public Transform joint;
    public Transform handle;

    void Update()
    {
        // TODO: account for angle wrap around / weird 180 flip behavior (gimbal lock?)

        outputX = Mathf.InverseLerp(minX, maxX, handle.rotation.eulerAngles.x);
        outputY = Mathf.InverseLerp(minY, maxY, handle.rotation.eulerAngles.y);
        outputZ = Mathf.InverseLerp(minZ, maxZ, handle.rotation.eulerAngles.z);

        joint.rotation = Quaternion.Euler(
            0f, // Mathf.Lerp(minX, maxX, outputX),
            Mathf.Lerp(minY, maxY, outputY),
            0f //Mathf.Lerp(minZ, maxZ, outputZ)
        );
    }
}
