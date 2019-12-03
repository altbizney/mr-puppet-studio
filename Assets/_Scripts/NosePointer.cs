using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet.WIP
{
    public class NosePointer : MonoBehaviour
    {
        public Transform noseJoint, camTarget;

        private Vector3 camTargetOnPlane;
        private Quaternion rotationToCamTarget;

        public float noseAngle = 90f;
        public float angleCutoff = 45f;

        [Range(-1f, 1f)]
        public float knob = 0f;

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

            // remap from range to INVERTED knob range
            knob = angleFromCenter.Remap(-angleCutoff, angleCutoff, -1f, 1f) * -1;

            // flop the actual nose
            noseJoint.localRotation = Quaternion.SlerpUnclamped(Quaternion.identity, Quaternion.Euler(0f, noseAngle, 0f), knob);

        }
    }
}