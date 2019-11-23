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
        [HideInTables] public Quaternion OriginalRotation, OriginalLocalRotation, OffsetRotation;
    }

    [InfoBox("Place this component on the stationary Transform which represents the orientation the destination character is facing")]
    public Transform SourceRoot;
    public Transform DestinationRoot;

    [TableList, HideInEditorMode]
    public List<BodyPart> BodyParts;

    private void Awake()
    {
        Component[] SourceBones = SourceRoot.GetComponentsInChildren(typeof(Transform), true);
        Component[] DestinationBones = DestinationRoot.GetComponentsInChildren(typeof(Transform), true);

        BodyParts = new List<BodyPart>();
        for (var i = 0; i < SourceBones.Length; i++)
        {
            BodyPart bp = new BodyPart();

            bp.Source = SourceBones[i] as Transform;
            bp.Destination = DestinationBones[i] as Transform;
            bp.OriginalRotation = bp.Destination.rotation;
            bp.OriginalLocalRotation = bp.Destination.localRotation;
            bp.OffsetRotation = Quaternion.Inverse(bp.Destination.rotation);

            BodyParts.Add(bp);
        }
    }

    private void LateUpdate()
    {
        foreach (BodyPart bp in BodyParts)
        {
            // bp.Destination.localRotation = bp.Source.localRotation;
            bp.Destination.localPosition = bp.Source.localPosition;

            // add handle onto source
            if (bp.Handle)
            {
                bp.Destination.Translate(bp.Handle.position, transform);
                bp.Destination.localRotation = bp.Handle.rotation;

                // bp.Destination.localRotation *= bp.Handle.localRotation; // mostly right but rotates "backwards" -- because its rotating on e.g. local red
                // bp.Destination.localRotation = Quaternion.Inverse(bp.Handle.rotation) * bp.Destination.localRotation; // BEST SO FAR -- rotates along local of BP -- needs to be relative to root transform
                // bp.Destination.localRotation = Quaternion.Inverse(bp.Handle.rotation) * bp.Source.localRotation;

                // VERY CLOSE: the -90 represents the diff from origin. this bends correctly, but faces wrong way. plus not dynamic.
                // TODO: calculate -90 automatically
                // TODO: don't disrupt Destination orientation
                // Quaternion q = bp.Handle.rotation * Quaternion.Euler(0f, -90f, 0f);
                // bp.Destination.localRotation = Quaternion.Inverse(q) * bp.Source.localRotation;
                // END VERY CLOSE

                // Quaternion diff = bp.OffsetRotation * transform.rotation;
                // bp.Destination.localRotation = diff * bp.Handle.rotation;

                // Quaternion q = bp.Handle.rotation * Quaternion.Inverse(bp.Source.localRotation);
                // Quaternion diff = bp.OriginalRotation * Quaternion.Inverse(transform.localRotation);
                // diff = q * diff;
                // bp.Destination.localRotation = diff;


                // bp.Destination.localRotation = Quaternion.Inverse(bp.Destination.localRotation) * bp.Handle.rotation;
                // bp.Destination.localRotation = Quaternion.Inverse(bp.Handle.rotation) * bp.Destination.localRotation; // rotates along local of BP -- needs to be for root transform

                // bone rotation = (source rotation) + (handle rotation, relative to root transform)
                // bp.Destination.localRotation = Quaternion.Inverse(bp.Destination.localRotation) * Quaternion.FromToRotation(bp.Handle.forward, transform.forward);
                // bp.Destination.localRotation *= Quaternion.FromToRotation(bp.Handle.forward, transform.forward);
                // bp.Destination.localRotation = Quaternion.Inverse(bp.Handle.rotation) * bp.Source.localRotation;

                // bp.Destination.localRotation = bp.Source.localRotation * bp.Handle.rotation;
                // bp.Destination.localPosition = bp.Source.localPosition + bp.Handle.position;
                // bp.Destination.localPosition = bp.Source.localPosition + bp.Handle.localPosition;
                // bp.Destination.localRotation = bp.Source.localRotation * bp.Handle.localRotation;

                // ---
                // bp.Destination.localRotation = Quaternion.Inverse(bp.Handle.rotation) * bp.Destination.localRotation;
                // inverse(starting) * movement

                /*
                Quaternion LocalRotation = Quaternion.Inverse(Target.transform.rotation) * WorldRotation;

                To get the difference C between quaternions A and B you do this:
                C = A * Quaternion.Inverse(B);

                To add the difference to D you do this:
                D = C * D;
                */

                // Quaternion C = transform.localRotation * Quaternion.Inverse(bp.Destination.localRotation);
                // Quaternion D = bp.Handle.rotation;
                // D = C * D;
                // bp.Destination.localRotation = D;

                // add handle rotation
                // correct for offset from root
                // Quaternion diff = transform.localRotation * Quaternion.Inverse(bp.Destination.localRotation);
                // bp.Destination.localRotation = diff * Quaternion.Inverse(bp.Handle.rotation) * bp.Destination.localRotation;

                // ---

                // bp.Destination.localRotation = bp.Handle.rotation * bp.Destination.localRotation;
                // DebugGraph.Log(Quaternion.Angle(transform.rotation, bp.Destination.rotation));
                // Quaternion q = bp.Destination.rotation;
                // q.SetLookRotation(transform.forward, transform.up);

                Debug.DrawRay(bp.Handle.position, bp.Handle.right, Color.red, 0.1f);
                Debug.DrawRay(bp.Handle.position, bp.Handle.forward, Color.blue, 0.1f);
                Debug.DrawRay(bp.Handle.position, bp.Handle.up, Color.green, 0.1f);
            }
            else
            {
                // copy as-is
                bp.Destination.localRotation = bp.Source.localRotation;
            }



            Debug.DrawRay(bp.Destination.position, bp.Destination.right * 0.15f, Color.red);
            Debug.DrawRay(bp.Destination.position, bp.Destination.forward * 0.15f, Color.blue);
            Debug.DrawRay(bp.Destination.position, bp.Destination.up * 0.15f, Color.green);
        }

        Debug.DrawRay(transform.position, transform.right, Color.red, 0.1f);
        Debug.DrawRay(transform.position, transform.forward, Color.blue, 0.1f);
        Debug.DrawRay(transform.position, transform.up, Color.green, 0.1f);
    }
}
