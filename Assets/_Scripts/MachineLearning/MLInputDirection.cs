using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLInputDirection : MonoBehaviour
{

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
            transform.Rotate(Vector3.down * 90f * Time.deltaTime);

        if (Input.GetKey(KeyCode.D))
            transform.Rotate(Vector3.up * 90f * Time.deltaTime);

        if (Input.GetKey(KeyCode.S))
            transform.position += transform.forward * Time.deltaTime * 4f;

        if (Input.GetKey(KeyCode.W))
            transform.position += (transform.forward * -1f) * Time.deltaTime * 4f;
    }
}
