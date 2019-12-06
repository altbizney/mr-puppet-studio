using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet
{
    public class NosePointer : MonoBehaviour
    {
        public Transform noseJoint, camTarget;

        private Vector3 camTargetOnPlane;
        private Quaternion rotationToCamTarget;

        [Range(0f, 180f)]
        public float hotspotSize = 3f;

        [Range(0f, 180f)]
        public float angleCutoff = 80f;

        [Range(0f, 180f)]
        public float noseExtremeAngle = 55f;

        [Range(-1f, 1f), ReadOnly]
        public float knobTarget, knobCurr = 0f;
        public float knobSmoothTime = 0.2f;

        private float knobVelocity;
        private float angleFromCenter;

        public bool EnableDebugGraph = false;

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
            // slerp unclamped, so -1 is extreme left, 0 is identity, and 1 is extreme right
            noseJoint.localRotation = Quaternion.SlerpUnclamped(Quaternion.identity, Quaternion.Euler(0f, noseExtremeAngle, 0f), knobCurr);
        }

        private void OnDisable()
        {
            noseJoint.localRotation = Quaternion.identity;
        }
    }
}