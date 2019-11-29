using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPuppet
{
    public class ClampTest : MonoBehaviour
    {
        [HorizontalGroup("Split", LabelWidth = 20)]

        [BoxGroup("Split/Min")]
        [LabelText("X")]
        [Range(-179f, 0f)]
        public float minX = -179f;

        [BoxGroup("Split/Max")]
        [LabelText("X")]
        [Range(0f, 179f)]
        public float maxX = 179f;

        [BoxGroup("Split/Min")]
        [LabelText("Y")]
        [Range(-179f, 0f)]
        public float minY = -179f;

        [BoxGroup("Split/Max")]
        [LabelText("Y")]
        [Range(0f, 179f)]
        public float maxY = 179f;

        [BoxGroup("Split/Min")]
        [LabelText("Z")]
        [Range(-179f, 0f)]
        public float minZ = -179f;

        [BoxGroup("Split/Max")]
        [LabelText("Z")]
        [Range(0f, 179f)]
        public float maxZ = 179f;

        private Vector3 starting;

        private void Start()
        {
            starting = transform.localRotation.eulerAngles;
        }

        private void LateUpdate()
        {
            Vector3 diff = EulerAnglesBetween(starting, transform.localRotation.eulerAngles);

            transform.localRotation = Quaternion.Euler(
                starting.x + Mathf.Clamp(diff.x, minX, maxX),
                starting.y + Mathf.Clamp(diff.y, minY, maxY),
                starting.z + Mathf.Clamp(diff.z, minZ, maxZ)
            );
        }

        // https://answers.unity.com/questions/599393/angles-from-quaternionvector-problem.html
        private Vector3 EulerAnglesBetween(Vector3 from, Vector3 to)
        {
            Vector3 delta = to - from;

            if (delta.x > 180) delta.x -= 360;
            else if (delta.x < -180) delta.x += 360;

            if (delta.y > 180) delta.y -= 360;
            else if (delta.y < -180) delta.y += 360;

            if (delta.z > 180) delta.z -= 360;
            else if (delta.z < -180) delta.z += 360;

            return delta;
        }
    }
}
