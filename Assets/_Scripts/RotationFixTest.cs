using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Thinko;

public class RotationFixTest : MonoBehaviour
{
    public RealPuppetDataProvider DataProvider;
    public RealPuppetDataProvider.Source source;

    void Update()
    {
        transform.rotation = DataProvider.GetInput(source);
    }
}
