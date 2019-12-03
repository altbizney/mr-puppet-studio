using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet.WIP
{
    public class NosePointer : MonoBehaviour
    {
        public Transform joint, target;

        private Vector3 targetOnPlane;
        private Quaternion rotationToTarget;

        public float deadzone = 0.1f;
        public float noseAngle = 90f;
        public float angleCutoff = 45f;

        [Range(-1f, 1f)]
        public float knob = 0f;

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            Debug.DrawLine(joint.position, target.position, Color.white);
            Gizmos.DrawSphere(joint.position, 0.1f);
            Gizmos.DrawSphere(target.position, 0.1f);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(joint.position, targetOnPlane);
            Gizmos.DrawSphere(targetOnPlane, 0.1f);
        }

        private void LateUpdate()
        {
            targetOnPlane = new Vector3(target.position.x, joint.position.y, target.position.z);
            rotationToTarget = Quaternion.LookRotation(targetOnPlane - joint.position);

            joint.localRotation = Quaternion.SlerpUnclamped(Quaternion.identity, Quaternion.Euler(0f, noseAngle, 0f), knob);
        }
    }
}