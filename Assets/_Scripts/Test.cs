using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Test : MonoBehaviour
{
    [Serializable]
    public class BodyPart
    {
        public Transform Source;
        public Transform Destination;
        public Transform Handle;
        // [HideInTables]
        // public Vector3 InitialPosition;
        // [HideInTables]
        // public Quaternion InitialRotation;
    }

    public Transform SourceRoot;
    public Transform DestinationRoot;
    [TableList] public List<BodyPart> BodyParts;

    private void Awake()
    {
        Component[] SourceBones = SourceRoot.GetComponentsInChildren(typeof(Transform));
        Component[] DestinationBones = DestinationRoot.GetComponentsInChildren(typeof(Transform));

        BodyParts = new List<BodyPart>();
        for (var i = 0; i < SourceBones.Length; i++)
        {
            BodyPart bp = new BodyPart();

            bp.Source = SourceBones[i] as Transform;
            bp.Destination = DestinationBones[i] as Transform;

            // bp.InitialPosition = bp.Destination.localPosition;
            // bp.InitialRotation = bp.Destination.localRotation;

            BodyParts.Add(bp);
        }
    }

    private void LateUpdate()
    {
        // Joint.localPosition = transform.position;
        foreach (BodyPart bp in BodyParts)
        {
            if (bp.Handle)
            {
                // add handle onto source
                bp.Destination.localPosition = bp.Source.localPosition + bp.Handle.localPosition;
                bp.Destination.localRotation = bp.Source.localRotation * bp.Handle.localRotation;
            }
            else
            {
                // copy directly
                bp.Destination.localPosition = bp.Source.localPosition;
                bp.Destination.localRotation = bp.Source.localRotation;
            }

            // bp.Destination.localPosition = bp.InitialPosition + bp.Source.localPosition;
            // bp.Destination.localRotation = Quaternion.Inverse(bp.Source.localRotation) * bp.InitialRotation;
        }
    }
}
