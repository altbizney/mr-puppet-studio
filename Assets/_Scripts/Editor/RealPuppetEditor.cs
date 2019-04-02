using UnityEditor;
using UnityEngine;

namespace Thinko
{
    [CustomEditor(typeof(RealPuppet))]
    public class RealPuppetEditor : Editor
    {
        private RealPuppet _realPuppet;

        private Transform[] _childNodes;

        private void OnEnable()
        {
            _realPuppet = target as RealPuppet;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            
            var realPuppet = (RealPuppet)target;

            serializedObject.Update();
            
            // Head
            GUILayout.Space(10);
            GUILayout.Label("HEAD", EditorStyles.whiteLargeLabel);
            
            realPuppet.AnimateHead = EditorGUILayout.Toggle("Enable", realPuppet.AnimateHead);
            if (realPuppet.AnimateHead)
            {
                EditorGUI.indentLevel = 1;
                realPuppet.HeadNode = EditorGUILayout.ObjectField("Node", realPuppet.HeadNode, typeof(Transform), true) as Transform;
                realPuppet.HeadRotationOffset = EditorGUILayout.Vector3Field("Rotation Offset", realPuppet.HeadRotationOffset);
                realPuppet.HeadRotationSharpness = EditorGUILayout.Slider("Rotation Sharpness", realPuppet.HeadRotationSharpness, 0, 1);
                realPuppet.DebugDrawRotation = EditorGUILayout.Toggle("Draw Rotation Gizmo", realPuppet.DebugDrawRotation);
            }
            EditorGUI.indentLevel = 0;
            
            GUILayout.Space(10);
            
            // Jaw
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Label("JAW", EditorStyles.whiteLargeLabel);
            
            realPuppet.AnimateJaw = EditorGUILayout.Toggle("Enable", realPuppet.AnimateJaw);
            if (realPuppet.AnimateJaw)
            {
                EditorGUI.indentLevel = 1;
                realPuppet.JawNode = EditorGUILayout.ObjectField("Node", realPuppet.JawNode, typeof(Transform), true) as Transform;
                realPuppet.JawInitialPose = EditorGUILayout.ObjectField("Initial Pose", realPuppet.JawInitialPose, typeof(Transform), true) as Transform;
                realPuppet.JawExtremePose = EditorGUILayout.ObjectField("Extreme Pose", realPuppet.JawExtremePose, typeof(Transform), true) as Transform;

                GUILayout.Space(10);
                GUI.contentColor = Color.green;
                EditorGUILayout.LabelField($"Live Glove Value: {realPuppet.JawGlove}");
                GUI.contentColor = Color.white;
                
                EditorGUILayout.BeginHorizontal();
                realPuppet.JawMin = EditorGUILayout.FloatField("Min Glove Value", realPuppet.JawMin);
                GUI.enabled = Application.isPlaying;
                if (GUILayout.Button("Grab"))
                {
                    realPuppet.JawMin = realPuppet.JawGlove;
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                realPuppet.JawMax = EditorGUILayout.FloatField("Max Glove Value", realPuppet.JawMax);
                GUI.enabled = Application.isPlaying;
                if (GUILayout.Button("Grab"))
                {
                    realPuppet.JawMax = realPuppet.JawGlove; 
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(10);
                realPuppet.JawSmoothness = EditorGUILayout.Slider("Anim Smoothness", realPuppet.JawSmoothness, 0, .3f);
            }
            EditorGUI.indentLevel = 0;

            serializedObject.ApplyModifiedProperties();
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(realPuppet);
            }
        }

        private void OnSceneGUI()
        {
            if (!_realPuppet.enabled)
                return;


            // Draw child and root nodes
            if (_realPuppet.RootNode)
            {
                if(_childNodes == null)
                    _childNodes = _realPuppet.RootNode.GetComponentsInChildren<Transform>();
                
                var handleSize = HandleUtility.GetHandleSize(Vector3.zero) * .1f;
                foreach (var child in _childNodes)
                {
                    Handles.color = child == _realPuppet.RootNode ? Color.magenta : Color.yellow;
                    Handles.CircleHandleCap(0, child.position, Quaternion.identity, handleSize, EventType.Repaint);
                }
            }

            // Draw main nodes
            if (_realPuppet.HeadNode)
                DrawMainNode(_realPuppet.HeadNode, "Head", Color.yellow);

            if (_realPuppet.ButtNode)
                DrawMainNode(_realPuppet.ButtNode, "Butt", Color.yellow);

            if (_realPuppet.JawNode)
                DrawMainNode(_realPuppet.JawNode, "Jaw", Color.yellow);
        }

        private void DrawMainNode(Transform node, string nodeName, Color color)
        {
            var position = node.position;

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
                Undo.RecordObject(node, "Move");
                node.position = newPos;
            }
        }
    }
}