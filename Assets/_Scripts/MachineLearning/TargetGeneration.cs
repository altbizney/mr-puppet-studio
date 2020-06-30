using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetGeneration : MonoBehaviour
{
    public GameObject MLTarget;
    void Start()
    {
        for (int x = -10; x < 10; x += 2)
        {
            for (int y = -10; y < 10; y += 2)
            {
                Instantiate(MLTarget, new Vector3(x, 0, y), Quaternion.identity);
            }
        }

    }
}
