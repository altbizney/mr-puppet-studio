using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLController : MonoBehaviour
{
    public GameObject Actor;
    public GameObject Pose;

    void Awake()
    {
        for (float x = -3f; x < 3f; x += 0.5f)
        {
            for (float z = -3f; z < 3f; z += 0.5f)
            {
                Instantiate(Pose, new Vector3(x, Pose.transform.position.y, z), Actor.transform.rotation);
            }
        }

    }

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            Instantiate(Pose, new Vector3(Actor.transform.position.x, Pose.transform.position.y, Actor.transform.position.z), Actor.transform.rotation);
        }
    }
}