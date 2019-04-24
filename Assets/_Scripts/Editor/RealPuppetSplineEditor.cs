using UnityEditor;
using UnityEngine;

namespace Thinko
{
    [CustomEditor(typeof(RealPuppetSpline))]
    public class RealPuppetSplineEditor : Editor
    {
        private RealPuppetSpline _realPuppetSpline;

        private void OnEnable()
        {
            _realPuppetSpline = target as RealPuppetSpline;
        }
        
        private void OnSceneGUI()
        {
            if (!_realPuppetSpline.enabled)
                return;
            
            
            foreach (var joint in _realPuppetSpline.Joints)
            {
                DrawJoint(joint, joint.name, Color.yellow);
            }
        }
        
        private void DrawJoint(Transform joint, string nodeName, Color color)
        {
            if(joint == null) return;
            var position = joint.position;

            // Label
            GUI.contentColor = color;
            Handles.Label(position, nodeName, new GUIStyle()
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleLeft,
                contentOffset = new Vector2(15, -10),
                normal = new GUIStyleState()
                {
                    textColor = Color.yellow
                }
            });

            // Move handle
            EditorGUI.BeginChangeCheck();
            var newPos = Handles.FreeMoveHandle(
                position,
                Quaternion.identity,
                HandleUtility.GetHandleSize(Vector3.zero) * .1f,
                Vector3.zero,
                Handles.SphereHandleCap);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(joint, "Move");
                joint.position = newPos;
            }
        }
    }
}