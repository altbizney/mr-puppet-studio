using UnityEditor;
using UnityEngine;

namespace Thinko
{
    [CustomPropertyDrawer(typeof(RealPuppet.PuppetJoint))]
    public class PuppetJointDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight( SerializedProperty property, GUIContent label ) 
        {
            return EditorGUIUtility.singleLineHeight * 6;
        }
        
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);

            var defColor = GUI.color;
            
            var x = rect.x;
            rect.y += 10;
            
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, 35, EditorGUIUtility.singleLineHeight), 
                property.FindPropertyRelative("Enabled"), GUIContent.none);
            
            GUI.color = property.FindPropertyRelative("RealPuppetDataProvider").objectReferenceValue != null ? defColor : Color.red;
            rect.x += 35;
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), 
                property.FindPropertyRelative("RealPuppetDataProvider"), GUIContent.none);
            GUI.color = defColor;
            
            rect.x += 200;
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width - 235, EditorGUIUtility.singleLineHeight), 
                property.FindPropertyRelative("InputSource"), GUIContent.none);
            
            GUI.color = property.FindPropertyRelative("Joint").objectReferenceValue != null ? defColor : Color.red;
            rect.x = x;
            rect.y += EditorGUIUtility.singleLineHeight * 1.25f;
            EditorGUI.ObjectField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), 
                property.FindPropertyRelative("Joint"),
                new GUIContent("Joint"));
            GUI.color = defColor;

            rect.x = x;
            rect.y += EditorGUIUtility.singleLineHeight * 1.25f;
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), 
                property.FindPropertyRelative("Offset"), 
                new GUIContent("Offset"));
            
            rect.y += EditorGUIUtility.singleLineHeight * 1.25f;
            EditorGUI.Slider(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), 
                property.FindPropertyRelative("Sharpness"), 
                0, 1, new GUIContent("Sharpness"));

            EditorGUI.EndProperty();
        }
    }
    
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
            
            // Joints
            GUILayout.Space(10);
            GUILayout.Label("JOINTS", EditorStyles.whiteLargeLabel);

            // Add joint button
            if (GUILayout.Button("Add Joint"))
            {
                _realPuppet.PuppetJoints.Add(new RealPuppet.PuppetJoint()
                {
                    Enabled = true
                });
            }
            
            // Joints list
            EditorGUILayout.PropertyField(serializedObject.FindProperty("PuppetJoints"), true);
            
            // Jaw
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUILayout.Label("JAW", EditorStyles.whiteLargeLabel);
            
            _realPuppet.AnimateJaw = EditorGUILayout.Toggle("Enable", _realPuppet.AnimateJaw);
            if (_realPuppet.AnimateJaw)
            {
                EditorGUI.indentLevel = 1;
                _realPuppet.JawRealPuppetDataProvider = EditorGUILayout.ObjectField("Jaw Data Provider", _realPuppet.JawRealPuppetDataProvider, typeof(RealPuppetDataProvider), true) as RealPuppetDataProvider;
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
            
            
            foreach (var puppetJoint in _realPuppet.PuppetJoints)
            {
                if (!puppetJoint.Enabled || puppetJoint.RealPuppetDataProvider == null || puppetJoint.Joint == null) continue;
                
                var handleSize = HandleUtility.GetHandleSize(puppetJoint.Joint.position) * .1f;
                Handles.CircleHandleCap(0, puppetJoint.Joint.position, Quaternion.identity, handleSize, EventType.Repaint);
                
                // Label
                GUI.contentColor = Color.yellow;
                Handles.Label(puppetJoint.Joint.position, puppetJoint.InputSource.ToString(), new GUIStyle()
                {
                    fontSize = 20,
                    alignment = TextAnchor.MiddleLeft,
                    contentOffset = new Vector2(15, -10),
                    normal = new GUIStyleState()
                    {
                        textColor = Color.yellow
                    }
                });
                GUI.contentColor = Color.white;
            }

//            // Draw child and root nodes
//            if (_realPuppet.RootNode)
//            {
//                if(_childNodes == null)
//                    _childNodes = _realPuppet.RootNode.GetComponentsInChildren<Transform>();
//                
//                var handleSize = HandleUtility.GetHandleSize(Vector3.zero) * .1f;
//                foreach (var child in _childNodes)
//                {
//                    Handles.color = child == _realPuppet.RootNode ? Color.magenta : Color.yellow;
//                    Handles.CircleHandleCap(0, child.position, Quaternion.identity, handleSize, EventType.Repaint);
//                }
//            }
//
//            // Draw main nodes
//            if (_realPuppet.HeadNode)
//                DrawMainNode(_realPuppet.HeadNode, "Head", Color.yellow);
//
//            if (_realPuppet.ButtNode)
//                DrawMainNode(_realPuppet.ButtNode, "Butt", Color.yellow);
//
//            if (_realPuppet.JawNode)
//                DrawMainNode(_realPuppet.JawNode, "Jaw", Color.yellow);
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