using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinBone : MonoBehaviour
{
    private Quaternion rot;
    private Vector3 pos;

    public Transform bone;

    private void Awake()
    {
        rot = bone.rotation;
        pos = bone.position;
    }

    private void Update() {
        bone.rotation = rot;
        bone.position = pos;
    }
}
