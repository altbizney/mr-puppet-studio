using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLPillarScript : MonoBehaviour
{
    public GameObject[] MyAgents = new GameObject[4];

    void Update()
    {
        float Height = 0;
        foreach (GameObject agent in MyAgents)
        {
            Height += agent.GetComponent<MLRotation>().MyScore;
        }
        gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, Height, gameObject.transform.localScale.x);

    }
}
