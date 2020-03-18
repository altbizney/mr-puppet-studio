using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRotationModifier : MonoBehaviour
{
    [Range(1f, 10f)]
    public float modifier = 1f;

    public Transform other;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!other) return;

        Vector3 rot = transform.rotation.eulerAngles;

        // breaks at wraparound!
        other.transform.eulerAngles = rot * modifier;
    }
}
