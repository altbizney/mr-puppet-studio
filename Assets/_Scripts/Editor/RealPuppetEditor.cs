using UnityEditor;
using UnityEngine;

namespace Thinko
{
    [CustomEditor(typeof(RealPuppet))]
    public class RealPuppetEditor : Editor
    {
        private RealPuppet _realPuppet;

        private Transform[] _childNodes;

        private static float _playModeJawMin;
        private static float _playModeJawMax;

        private void OnEnable()
        {
            _realPuppet = target as RealPuppet;
            
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            // Save play mode jaw values
            if (obj == PlayModeStateChange.ExitingPlayMode)
            {
                _playModeJawMin = _realPuppet.JawMin;
                _playModeJawMax = _realPuppet.JawMax;
            }
            else if (obj == PlayModeStateChange.EnteredEditMode)
            {
                _realPuppet.JawMin = _playModeJawMin;
                _realPuppet.JawMax = _playModeJawMax;
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            
            serializedObject.Update();
            
            GUILayout.Space(10);
            
            // Data Provider
            _realPuppet.RealPuppetDataProvider = EditorGUILayout.ObjectField("Data Provider", _realPuppet.RealPuppetDataProvider, typeof(RealPuppetDataProvider), true) as RealPuppetDataProvider;

            // Data Provider
            _realPuppet.body = EditorGUILayout.ObjectField("Body", _realPuppet.body, typeof(Transform), true) as Transform;

            // Head
            GUILayout.Space(10);
            GUILayout.Label("HEAD", EditorStyles.whiteLargeLabel);
            
            _realPuppet.AnimateHead = EditorGUILayout.Toggle("Enable", _realPuppet.AnimateHead);
            if (_realPuppet.AnimateHead)
            {
                EditorGUI.indentLevel = 1;
                _realPuppet.HeadNode = EditorGUILayout.ObjectField("Node", _realPuppet.HeadNode, typeof(Transform), true) as Transform;
                _realPuppet.HeadRotationOffset = EditorGUILayout.Vector3Field("Rotation Offset", _realPuppet.HeadRotationOffset);
                _realPuppet.HeadRotationSharpness = EditorGUILayout.Slider("Rotation Sharpness", _realPuppet.HeadRotationSharpness, 0, 1);
                _realPuppet.DebugDrawRotation = EditorGUILayout.Toggle("Draw Rotation Gizmo", _realPuppet.DebugDrawRotation);
            }
            EditorGUI.indentLevel = 0;
            
            GUILayout.Space(10);
            
            // Jaw
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Label("JAW", EditorStyles.whiteLargeLabel);
            
            _realPuppet.AnimateJaw = EditorGUILayout.Toggle("Enable", _realPuppet.AnimateJaw);
            if (_realPuppet.AnimateJaw)
            {
                EditorGUI.indentLevel = 1;
                _realPuppet.JawNode = EditorGUILayout.ObjectField("Node", _realPuppet.JawNode, typeof(Transform), true) as Transform;
                _realPuppet.JawInitialPose = EditorGUILayout.ObjectField("Initial Pose", _realPuppet.JawInitialPose, typeof(Transform), true) as Transform;
                _realPuppet.JawExtremePose = EditorGUILayout.ObjectField("Extreme Pose", _realPuppet.JawExtremePose, typeof(Transform), true) as Transform;

                GUILayout.Space(10);
                GUI.contentColor = Color.green;
                EditorGUILayout.LabelField($"Live Glove Value: {_realPuppet.JawGlove}");
                GUI.contentColor = Color.white;
                
                EditorGUILayout.BeginHorizontal();
                _realPuppet.JawMin = EditorGUILayout.FloatField("Min Glove Value", _realPuppet.JawMin);
                GUI.enabled = Application.isPlaying;
                if (GUILayout.Button("Grab"))
                {
                    _realPuppet.JawMin = _realPuppet.JawGlove;
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                _realPuppet.JawMax = EditorGUILayout.FloatField("Max Glove Value", _realPuppet.JawMax);
                GUI.enabled = Application.isPlaying;
                if (GUILayout.Button("Grab"))
                {
                    _realPuppet.JawMax = _realPuppet.JawGlove; 
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
                
                GUILayout.Space(10);
                _realPuppet.JawSmoothness = EditorGUILayout.Slider("Anim Smoothness", _realPuppet.JawSmoothness, 0, .3f);
            }
            EditorGUI.indentLevel = 0;

            serializedObject.ApplyModifiedProperties();
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_realPuppet, "Modified RealPuppet Component"); 
                EditorUtility.SetDirty(_realPuppet);
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