using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRotationModifier : MonoBehaviour
{
    [Range(1f, 10f)]
    public float modifier = 1f;

    public Transform other;
    //Vector3 rot = Vector3.zero;
    Quaternion start;


    // Start is called before the first frame update
    void Start()
    {
        //transform.eulerAngles = new Vector3(180, 0, 0);
        //Debug.Log(transform.eulerAngles);

        start = transform.rotation;
        
    }

float ClampAngle(float angle ) {
    if (angle < -360.0f)
        angle += 360.0f;
    if (angle > 360.0f)
        angle -= 360.0f;
    return Mathf.Clamp(angle, 0, 360);
}

public static float CalcEulerSafeX(float x)
{
        if (x >= -90 && x <= 90)
            return x;
        x = x % 180;
        if (x > 0)
            x -= 180;
        else
            x += 180;
        return x;
}

    // Update is called once per frame
    void Update()
    {
        if (!other) return;
        Vector3 rot = transform.eulerAngles;
        //rot.x = CalcEulerSafeX(rot.x);
        //rot.y = CalcEulerSafeX(rot.y);
        //rot.z = CalcEulerSafeX(rot.z);

        //Debug.Log("MyAngle: " + rot + "OtherAngle :" + other.transform.eulerAngles);

        transform.rotation = Quaternion.SlerpUnclamped(start, transform.rotation, modifier);
        //Debug.Log(Quaternion.identity);


        // breaks at wraparound!
        //rot += new Vector3(10, 0, 0) * Time.deltaTime;
        //other.transform.eulerAngles = rot * modifier;
        //other.transform.eulerAngles = new Vector3(CalcEulerSafeX(other.transform.eulerAngles.x), CalcEulerSafeX(other.transform.eulerAngles.y), CalcEulerSafeX(other.transform.eulerAngles.z));
    }
}
