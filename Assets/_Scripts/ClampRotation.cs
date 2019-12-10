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
        [System.Flags]
        public enum ClampAxis
        {
            // None = 0,
            X = 1 << 1,
            Y = 1 << 2,
            Z = 1 << 3,
            // All = X | Y | Z
        }

        [EnumToggleButtons, HideLabel]
        public ClampAxis EnabledAxis = ClampAxis.X | ClampAxis.Y | ClampAxis.Z;

        [HorizontalGroup("Split", LabelWidth = 20)]

        [BoxGroup("Split/Min")]
        [LabelText("X")]
        [Range(-179f, 0f)]
        [EnableIf("IsClampedX")]
        public float minX = -179f;

        [BoxGroup("Split/Max")]
        [LabelText("X")]
        [Range(0f, 179f)]
        [EnableIf("IsClampedX")]
        public float maxX = 179f;

        [BoxGroup("Split/Min")]
        [LabelText("Y")]
        [Range(-179f, 0f)]
        [EnableIf("IsClampedY")]
        public float minY = -179f;

        [BoxGroup("Split/Max")]
        [LabelText("Y")]
        [Range(0f, 179f)]
        [EnableIf("IsClampedY")]
        public float maxY = 179f;

        [BoxGroup("Split/Min")]
        [LabelText("Z")]
        [Range(-179f, 0f)]
        [EnableIf("IsClampedZ")]
        public float minZ = -179f;

        [BoxGroup("Split/Max")]
        [LabelText("Z")]
        [Range(0f, 179f)]
        [EnableIf("IsClampedZ")]
        public float maxZ = 179f;

        private Vector3 starting, curr, diff;

        private void Start()
        {
            starting = transform.localRotation.eulerAngles;
        }

        private void LateUpdate()
        {
            curr = transform.localRotation.eulerAngles;
            diff = EulerAnglesBetween(curr - starting);

            transform.localRotation = Quaternion.Euler(
                IsClampedX() ? starting.x + Mathf.Clamp(diff.x, minX, maxX) : curr.x,
                IsClampedY() ? starting.y + Mathf.Clamp(diff.y, minY, maxY) : curr.y,
                IsClampedZ() ? starting.z + Mathf.Clamp(diff.z, minZ, maxZ) : curr.z
            );
        }

        // https://answers.unity.com/questions/599393/angles-from-quaternionvector-problem.html
        private Vector3 EulerAnglesBetween(Vector3 delta)
        {
            if (delta.x > 180) delta.x -= 360;
            else if (delta.x < -180) delta.x += 360;

            if (delta.y > 180) delta.y -= 360;
            else if (delta.y < -180) delta.y += 360;

            if (delta.z > 180) delta.z -= 360;
            else if (delta.z < -180) delta.z += 360;

            return delta;
        }

        private Vector3 EulerAnglesBetween(Vector3 from, Vector3 to)
        {
            return EulerAnglesBetween(to - from);
        }

        public bool IsClamped(ClampAxis flag)
        {
            return (EnabledAxis & flag) == flag;
        }

        private bool IsClampedX() { return IsClamped(ClampAxis.X); }
        private bool IsClampedY() { return IsClamped(ClampAxis.Y); }
        private bool IsClampedZ() { return IsClamped(ClampAxis.Z); }
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
