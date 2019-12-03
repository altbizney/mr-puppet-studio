using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet.WIP
{
    // https://www.weaverdev.io/blog/bonehead-procedural-animation
    public class LookAtTarget : MonoBehaviour
    {
        public Transform target;
        public Transform headBone;
        public float headMaxTurnAngle = 180f;
        public float headTrackingSpeed = 10f;

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

        public Vector3 offset = new Vector3(0f, 0f, 0f);

        private void LateUpdate()
        {
            Vector3 targetWorldLookDir = target.position - headBone.position;
            Vector3 targetLocalLookDir = headBone.parent.InverseTransformDirection(targetWorldLookDir);

            headBone.localRotation = Quaternion.LookRotation(targetLocalLookDir, headBone.parent.InverseTransformDirection(UpDirection)) * Quaternion.Euler(offset);

            Debug.DrawLine(target.position, headBone.position, Color.white);

            // // Store the current head rotation since we will be resetting it
            // Quaternion currentLocalRotation = headBone.localRotation;

            // // Reset the head rotation so our world to local space transformation will use the head's zero rotation. 
            // // Note: Quaternion.Identity is the quaternion equivalent of "zero"
            // headBone.localRotation = Quaternion.identity;

            // Vector3 targetWorldLookDir = target.position - headBone.position;
            // Vector3 targetLocalLookDir = headBone.InverseTransformDirection(targetWorldLookDir);
            // // Vector3 targetLocalLookDir = headBone.parent.InverseTransformDirection(targetWorldLookDir);


            // // Apply angle limit
            // targetLocalLookDir = Vector3.RotateTowards(
            //   ForwardDirection,
            //   targetLocalLookDir,
            //   Mathf.Deg2Rad * headMaxTurnAngle, // Note we multiply by Mathf.Deg2Rad here to convert degrees to radians
            //   0f // We don't care about the length here, so we leave it at zero
            // );

            // // Get the local rotation by using LookRotation on a local directional vector
            // Quaternion targetLocalRotation = Quaternion.LookRotation(targetLocalLookDir, UpDirection);

            // // Apply smoothing
            // headBone.localRotation = Quaternion.Slerp(
            //   currentLocalRotation,
            //   targetLocalRotation,
            //   1 - Mathf.Exp(-headTrackingSpeed * Time.deltaTime)
            // );
        }
    }
}