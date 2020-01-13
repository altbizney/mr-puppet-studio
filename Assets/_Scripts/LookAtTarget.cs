using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet
{
    // https://www.weaverdev.io/blog/bonehead-procedural-animation
    public class LookAtTarget : MonoBehaviour
    {
        public Transform lookTarget;

        [ChildGameObjectsOnly]
        public Transform headJoint;

        private ValueDropdownList<Vector3> VectorDirectionValues() { return VectorTools.VectorDirectionValues; }

        [ValueDropdown("VectorDirectionValues", HideChildProperties = true)]
        public Vector3 UpDirection = Vector3.up;

        [OnValueChanged("CacheOffsetQuaternion")]
        public Vector3 offset = new Vector3(0f, 0f, 0f);
        private Quaternion offsetQuaternion;

        [Range(0f, 1f), ReadOnly]
        public float weight = 0f;
        [Range(0f, 1f)]
        public float weightMax = 1f;

        public float smoothTime = 0.1f;

        private float weightTarget, weightVelocity = 0f;

        public KeyCode weightKey = KeyCode.L;

        public bool DrawGizmos = false;

        private void OnEnable()
        {
            CacheOffsetQuaternion();
        }

        private void Update()
        {
            if (Input.GetKeyDown(weightKey)) weightTarget = weightMax;
            if (Input.GetKeyUp(weightKey)) weightTarget = 0f;
        }

        private void LateUpdate()
        {
            weight = Mathf.SmoothDamp(weight, weightTarget, ref weightVelocity, smoothTime);

            headJoint.localRotation = Quaternion.Slerp(
                headJoint.localRotation,
                Quaternion.LookRotation(
                    headJoint.parent.InverseTransformDirection(lookTarget.position - headJoint.position),
                    headJoint.parent.InverseTransformDirection(UpDirection)
                ) * offsetQuaternion,
                weight);
        }

        private void OnDrawGizmos()
        {
            if (!DrawGizmos || !lookTarget || !headJoint) return;

            Gizmos.color = Color.white;
            Gizmos.DrawLine(lookTarget.position, headJoint.position);
        }

        private void CacheOffsetQuaternion()
        {
            offsetQuaternion = Quaternion.Euler(offset);
        }
    }
}
