using System;
using Sirenix.OdinInspector;
using UnityEngine;
using RootMotion.FinalIK;


public class MojoOneShotCleanUp : MonoBehaviour
{

    private void Awake()
    {
        KillChildren(gameObject.GetComponentsInChildren<IKTag>());
        //KillChildren(gameObject.GetComponentsInChildren<TrigonoetricIK>());
        //do not kill grinder?
        //kill limb IK
        //delete reference head
    }

    private void KillChildren(UnityEngine.Object[] children)
    {
        foreach (UnityEngine.Object child in children)
            Destroy(child);
    }
}
