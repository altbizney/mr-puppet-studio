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

            DebugGraph.MultiLog(angleFromCenter, "angleFromCenter");
            DebugGraph.MultiLog(hotspotAngle, "hotspotAngle");
            DebugGraph.MultiLog(-hotspotAngle, "-hotspotAngle");
            DebugGraph.MultiLog(angleCutoff, "angleCutoff");
            DebugGraph.MultiLog(-angleCutoff, "-angleCutoff");

            knobTarget = Mathf.Abs(angleFromCenter).RemapAndClamp(0f, angleCutoff, -1f, 0f);
            if (Mathf.Abs(angleFromCenter) < hotspotAngle) knobTarget = hotspotKnob - 1f;

            if (angleFromCenter < 0f) knobTarget *= -1;


            // if (angleFromCenter > 0f)
            // {
            //     knobTarget = angleFromCenter.RemapAndClamp(0f, angleCutoff, -1f, 0f);
            //     if (angleFromCenter < hotspotAngle) knobTarget = hotspotKnob - 1f;
            // }
            // else
            // {
            //     knobTarget = angleFromCenter.RemapAndClamp(0f, -angleFromCenter, 0f, -1f);
            //     if (angleFromCenter > -hotspotAngle) knobTarget = 1f - hotspotKnob;
            // }

            DebugGraph.Log(knobTarget);

            /*
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
            */

            // spring the nose flop based on knobTarget
            knobCurr = Mathf.SmoothDamp(knobCurr, knobTarget, ref knobVelocity, knobSmoothTime);
            // slerp unclamped, so -1 is extreme left, 0 is identity, and 1 is extreme right
            noseJoint.localRotation = Quaternion.SlerpUnclamped(Quaternion.identity, Quaternion.Euler(0f, noseExtremeAngle, 0f), knobCurr);
        }

        private void OnDisable()
        {
            noseJoint.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Find some projected angle measure off some forward around some axis.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="forward"></param>
        /// <param name="axis"></param>
        /// <returns>Angle in degrees</returns>
        // https://github.com/lordofduct/spacepuppy-unity-framework-3.0/blob/master/SpacepuppyUnityFramework/Utils/VectorUtil.cs#L297
        public static float AngleOffAroundAxis(Vector3 v, Vector3 forward, Vector3 axis, bool clockwise = false)
        {
            Vector3 right;
            if (clockwise)
            {
                right = Vector3.Cross(forward, axis);
                forward = Vector3.Cross(axis, right);
            }
            else
            {
                right = Vector3.Cross(axis, forward);
                forward = Vector3.Cross(right, axis);
            }
            return Mathf.Atan2(Vector3.Dot(v, right), Vector3.Dot(v, forward)) * Mathf.Rad2Deg;
        }
    }
}