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

        private ValueDropdownList<Vector3> VectorDirectionValues = new ValueDropdownList<Vector3>()
        {
            {"Forward", Vector3.forward },
            {"Back",    Vector3.back    },
            {"Up",      Vector3.up      },
            {"Down",    Vector3.down    },
            {"Right",   Vector3.right   },
            {"Left",    Vector3.left    },
        };

        [ValueDropdown("VectorDirectionValues", HideChildProperties = true)]
        public Vector3 UpDirection = Vector3.up;

        [OnValueChanged("CacheOffsetQuaternion")]
        public Vector3 offset = new Vector3(0f, 0f, 0f);
        private Quaternion offsetQuaternion;

        [Range(0f, 1f)]
        public float weight = 1f;

        public bool DrawGizmos = false;

        private void OnEnable()
        {
            CacheOffsetQuaternion();
        }

        private void LateUpdate()
        {
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