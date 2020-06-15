using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLController : MonoBehaviour
{
    public GameObject Actor;
    public GameObject Pose;

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            Instantiate(Pose, new Vector3(Actor.transform.position.x, Pose.transform.position.y, Actor.transform.position.z), Actor.transform.rotation);
        }
    }
}