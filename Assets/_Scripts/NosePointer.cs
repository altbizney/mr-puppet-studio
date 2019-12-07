using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet
{
    public class NosePointer : MonoBehaviour
    {
        [Serializable]
        public class Snapshot
        {
            [HideInTables]
            public List<Transform> elements;

            [HideInTables]
            public List<Vector3> localPosition;

            [HideInTables]
            public List<Quaternion> localRotation;

            public Snapshot Capture(Transform root)
            {
                elements = new List<Transform>(root.GetComponentsInChildren<Transform>(true));

                localPosition = new List<Vector3>();
                localRotation = new List<Quaternion>();

                foreach (Transform element in elements)
                {
                    localPosition.Add(element.localPosition);
                    localRotation.Add(element.localRotation);
                }

                return this;
            }

            public static void Lerp(Snapshot from, Snapshot to, float t)
            {
                foreach (Transform element in from.elements)
                {
                    int i = from.elements.IndexOf(element);

                    if (element.GetInstanceID() != to.elements[i].GetInstanceID()) throw new Exception("Mismatched elements");

                    element.localPosition = Vector3.Lerp(from.localPosition[i], to.localPosition[i], t);
                    element.localRotation = Quaternion.Slerp(from.localRotation[i], to.localRotation[i], t);
                }
            }

            public void Activate()
            {
                foreach (Transform element in elements)
                {
                    int i = elements.IndexOf(element);
                    element.localPosition = localPosition[i];
                    element.localRotation = localRotation[i];
                }
            }
        }

        public Transform noseJoint, camTarget;

        private Vector3 camTargetOnPlane;
        private Quaternion rotationToCamTarget;

        [Range(0f, 180f)]
        public float hotspotSize = 3f;

        [Range(0f, 180f)]
        public float angleCutoff = 80f;

        // [Range(0f, 180f)]
        // public float noseExtremeAngle = 55f;

        [Range(-1f, 1f), ReadOnly]
        public float knobTarget, knobCurr = 0f;
        public float knobSmoothTime = 0.2f;

        private float knobVelocity;
        private float angleFromCenter;

        public bool EnableDebugGraph = false;

        [SerializeField, HideInInspector]
        private Snapshot centerSnapshot, extremeLeftSnapshot, extremeRightSnapshot;

        // centerSnapshot
        [Button(ButtonSizes.Medium)]
        [HorizontalGroup("centerSnapshot")]
        [DisableInPlayMode]
        public void CaptureCenter()
        {
            centerSnapshot = new Snapshot().Capture(noseJoint);
        }

        [Button(ButtonSizes.Medium, Name = "Activate")]
        [HorizontalGroup("centerSnapshot", Width = 0.25f)]
        [DisableInPlayMode]
        public void ActivateCenter()
        {
            centerSnapshot.Activate();
        }

        // extremeLeftSnapshot
        [Button(ButtonSizes.Medium)]
        [HorizontalGroup("extremeLeftSnapshot")]
        [DisableInPlayMode]
        [PropertyTooltip("The puppets own left")]
        public void CaptureExtremeLeft()
        {
            extremeLeftSnapshot = new Snapshot().Capture(noseJoint);
        }

        [Button(ButtonSizes.Medium, Name = "Activate")]
        [HorizontalGroup("extremeLeftSnapshot", Width = 0.25f)]
        [DisableInPlayMode]
        public void ActivateExtremeLeft()
        {
            extremeLeftSnapshot.Activate();
        }

        // extremeRightSnapshot
        [Button(ButtonSizes.Medium)]
        [HorizontalGroup("extremeRightSnapshot")]
        [DisableInPlayMode]
        [PropertyTooltip("The puppets own right")]
        public void CaptureExtremeRight()
        {
            extremeRightSnapshot = new Snapshot().Capture(noseJoint);
        }

        [Button(ButtonSizes.Medium, Name = "Activate")]
        [HorizontalGroup("extremeRightSnapshot", Width = 0.25f)]
        [DisableInPlayMode]
        public void ActivateExtremeRight()
        {
            extremeRightSnapshot.Activate();
        }

        // [DisableInEditorMode]
        // [Range(-1f, 1f)]
        // [OnValueChanged("LerpSnapshots")]
        // public float lerp = 0f;

        // private void LerpSnapshots()
        // {
        //     Snapshot.Lerp(centerSnapshot, lerp > 0f ? extremeRightSnapshot : extremeLeftSnapshot, Mathf.Abs(lerp));
        // }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            Debug.DrawLine(noseJoint.parent.position, camTarget.position, Gizmos.color = Color.grey);
            Gizmos.DrawSphere(noseJoint.parent.position, 0.1f);
            Gizmos.DrawSphere(camTarget.position, 0.1f);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(noseJoint.parent.position, camTargetOnPlane);
            Gizmos.DrawSphere(camTargetOnPlane, 0.1f);
        }

        private float FixAngleWraparound(float angle)
        {
            if (angle > 180) angle -= 360;
            else if (angle < -180) angle += 360;
            return angle;
        }

        private void LateUpdate()
        {
            // project camTarget X/Z onto noseJoint.parent Y plane
            camTargetOnPlane = new Vector3(camTarget.position.x, noseJoint.parent.position.y, camTarget.position.z);

            // compute rotation from projected point to noseJoint parent
            rotationToCamTarget = Quaternion.LookRotation(camTargetOnPlane - noseJoint.parent.position);

            // compute current angle from origin defined by rotationToCamTarget.y
            angleFromCenter = FixAngleWraparound(rotationToCamTarget.eulerAngles.y - noseJoint.parent.rotation.eulerAngles.y);

            // TODO: move
            float hotspotAngle = hotspotSize / 2f;
            float hotspotKnob = Mathf.InverseLerp(0f, angleCutoff, hotspotAngle);

            // compute knob as absolute value
            knobTarget = Mathf.Abs(angleFromCenter).RemapAndClamp(0f, angleCutoff, -1f, 0f);
            if (Mathf.Abs(angleFromCenter) < hotspotAngle) knobTarget = hotspotKnob - 1f;

            // match original sign
            if (angleFromCenter < 0f) knobTarget *= -1;

            // TODO: only allow swap between hotspotKnob when "changing direction"

            if (EnableDebugGraph)
            {
                DebugGraph.MultiLog(angleFromCenter, "angleFromCenter");
                DebugGraph.MultiLog(hotspotAngle, "hotspotAngle");
                DebugGraph.MultiLog(-hotspotAngle, "-hotspotAngle");
                DebugGraph.MultiLog(angleCutoff, "angleCutoff");
                DebugGraph.MultiLog(-angleCutoff, "-angleCutoff");
                DebugGraph.Log(knobTarget);
            }

            // spring the nose flop based on knobTarget
            knobCurr = Mathf.SmoothDamp(knobCurr, knobTarget, ref knobVelocity, knobSmoothTime);

            Snapshot.Lerp(centerSnapshot, knobCurr > 0f ? extremeRightSnapshot : extremeLeftSnapshot, Mathf.Abs(knobCurr));

            // slerp unclamped, so -1 is extreme left, 0 is identity, and 1 is extreme right
            //noseJoint.localRotation = Quaternion.SlerpUnclamped(Quaternion.identity, Quaternion.Euler(0f, noseExtremeAngle, 0f), knobCurr);
        }

        private void OnDisable()
        {
            noseJoint.localRotation = Quaternion.identity;
        }
    }
}