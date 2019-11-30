using System;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif

namespace MrPuppet
{
    public class ClampRotation : MonoBehaviour
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

#if UNITY_EDITOR && false
    [CustomEditor(typeof(ClampRotation))]
    public class ClampRotationEditor : OdinEditor
    {
        private float size = 0.75f;

        private void OnSceneGUI()
        {
            ClampRotation src = (ClampRotation) target;

            Handles.color = Color.red;
            Handles.DrawWireArc(src.transform.position, src.transform.right, src.transform.up, 360f, size);

            Handles.color = new Color(size, 0f, 0f, 0.25f);
            Handles.DrawSolidArc(src.transform.position, src.transform.right, -src.transform.up, src.minX, size);
            Handles.DrawSolidArc(src.transform.position, src.transform.right, -src.transform.up, src.maxX, size);

            Handles.color = Color.green;
            Handles.DrawWireArc(src.transform.position, src.transform.up, src.transform.right, 360f, size);

            Handles.color = new Color(0f, 1f, 0f, 0.25f);
            Handles.DrawSolidArc(src.transform.position, src.transform.up, src.transform.right, src.minY, size);
            Handles.DrawSolidArc(src.transform.position, src.transform.up, src.transform.right, src.maxY, size);

            Handles.color = Color.blue;
            Handles.DrawWireArc(src.transform.position, src.transform.forward, src.transform.up, 360f, size);

            Handles.color = new Color(0f, 0f, 1f, 0.25f);
            Handles.DrawSolidArc(src.transform.position, src.transform.forward, -src.transform.up, src.minZ, size);
            Handles.DrawSolidArc(src.transform.position, src.transform.forward, -src.transform.up, src.maxZ, size);

        }
    }

#endif
}
