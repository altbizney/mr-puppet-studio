using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLInputDirection : MonoBehaviour
{

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
            transform.position += transform.right * Time.deltaTime * 4f;

        if (Input.GetKey(KeyCode.D))
            transform.position += (transform.right * -1) * Time.deltaTime * 4f;

        if (Input.GetKey(KeyCode.S))
            transform.position += transform.forward * Time.deltaTime * 4f;

        if (Input.GetKey(KeyCode.W))
            transform.position += (transform.forward * -1f) * Time.deltaTime * 4f;
    }
}
