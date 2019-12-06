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

        public float hotspotSize = 3f;
        public float noseAngle = 55f;
        public float angleCutoff = 80f;

        [Range(-1f, 1f), ReadOnly]
        public float knobTarget, knobCurr = 0f;
        public float knobSmoothTime = 0.2f;

        private float knobVel;

        private float angleFromCenter;

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

        private void LateUpdate()
        {
            // project camTarget X/Z onto noseJoint.parent Y plane
            camTargetOnPlane = new Vector3(camTarget.position.x, noseJoint.parent.position.y, camTarget.position.z);

            // compute rotation from projected point to noseJoint parent
            rotationToCamTarget = Quaternion.LookRotation(camTargetOnPlane - noseJoint.parent.position);

            // compute current angle from origin defined by rotationToCamTarget.y
            angleFromCenter = rotationToCamTarget.eulerAngles.y - noseJoint.parent.rotation.eulerAngles.y;

            // remap from range to knobTarget range, then invert
            knobTarget = angleFromCenter.Remap(-angleCutoff, angleCutoff, -1f, 1f) * -1;

            // override hotspot and cutoff
            if (angleFromCenter > 0f)
            {
                if (angleFromCenter < hotspotSize / 2f)
                {
                    // angleFromCenter = hotspotSize / 2f;
                    // Debug.Log("HOTSPOT - RIGHT");
                }

                if (angleFromCenter > angleCutoff)
                {
                    // Debug.Log("CUTOFF - RIGHT");
                    // angleFromCenter = 0f;
                    knobTarget = 0f;
                }
            }

            if (angleFromCenter < 0f)
            {
                if (angleFromCenter > -hotspotSize / 2f)
                {
                    // angleFromCenter = -hotspotSize / 2f;
                    // Debug.Log("HOTSPOT - LEFT");
                }

                if (angleFromCenter < -angleCutoff)
                {
                    // Debug.Log("CUTOFF - LEFT");
                    // angleFromCenter = 0f;
                    knobTarget = 0f;
                }
            }

            // spring the nose flop based on knobTarget
            knobCurr = Mathf.SmoothDamp(knobCurr, knobTarget, ref knobVel, knobSmoothTime);
            noseJoint.localRotation = Quaternion.SlerpUnclamped(Quaternion.identity, Quaternion.Euler(0f, noseAngle, 0f), knobCurr);
        }

        private void OnDisable()
        {
            noseJoint.localRotation = Quaternion.identity;
        }
    }
}