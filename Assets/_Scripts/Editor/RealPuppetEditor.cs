using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Thinko
{
    [CustomEditor(typeof(RealPuppet))]
    public class RealPuppetEditor : Editor
    {
        private RealPuppet _realPuppet;

        private Transform[] _childNodes;

        private ReorderableList _jointsList;
        private ReorderableList _dynamicBonesList;

        private static float _playModeJawMin;
        private static float _playModeJawMax;

        private void OnEnable()
        {
            _realPuppet = target as RealPuppet;

            _realPuppet.DynamicBones = _realPuppet.GetComponentsInChildren<DynamicBone>().ToList();
            CreateDynamicBonesList();
            CreateJointsList();
            
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
            
            // Auto-create the spline
            _realPuppet.AutoCreateSpline = EditorGUILayout.Toggle("Auto-Create Spline", _realPuppet.AutoCreateSpline);
            
            // Joints
            GUILayout.Space(10);
            GUI.color = Color.yellow;
            GUILayout.Label("JOINTS", EditorStyles.whiteLargeLabel);
            GUI.color = Color.white;
            
            var joint = DropAreaGameObjectGUI("Drop Joint Bone Here".ToUpper());
            if (joint != null)
            {
                _realPuppet.PuppetJoints.Add(new RealPuppet.PuppetJoint()
                {
                    Enabled = true,
                    Joint = joint.transform
                });
                Repaint();
            }
            
            // Joints list
            GUILayout.Space(10);
            _jointsList.DoLayoutList();
            
            // Jaw
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUI.color = Color.yellow;
            GUILayout.Label("JAW", EditorStyles.whiteLargeLabel);
            GUI.color = Color.white;

            EditorGUI.indentLevel++;
            
            EditorGUILayout.BeginHorizontal();
            _realPuppet.AnimateJaw = EditorGUILayout.Toggle(_realPuppet.AnimateJaw, GUILayout.Width(35));
            _realPuppet.JawRealPuppetDataProvider = EditorGUILayout.ObjectField(_realPuppet.JawRealPuppetDataProvider, typeof(RealPuppetDataProvider), true) as RealPuppetDataProvider;
            EditorGUILayout.EndHorizontal();
            
            if (_realPuppet.AnimateJaw)
            {
                GUILayout.Space(10);    
                EditorGUILayout.BeginHorizontal();
                _realPuppet.JawAnimMode = (RealPuppet.PuppetJawAnimMode)EditorGUILayout.EnumPopup(_realPuppet.JawAnimMode, GUILayout.Width(150));

                if (_realPuppet.JawAnimMode == RealPuppet.PuppetJawAnimMode.Transform)
                {
                    EditorGUILayout.BeginVertical();
                    _realPuppet.JawNode = EditorGUILayout.ObjectField("Joint", _realPuppet.JawNode, typeof(Transform), true) as Transform;
                    _realPuppet.JawInitialPose = EditorGUILayout.ObjectField("Initial Pose", _realPuppet.JawInitialPose, typeof(Transform), true) as Transform;
                    _realPuppet.JawExtremePose = EditorGUILayout.ObjectField("Extreme Pose", _realPuppet.JawExtremePose, typeof(Transform), true) as Transform;
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    EditorGUILayout.BeginVertical();
                    _realPuppet.JawMeshRenderer = EditorGUILayout.ObjectField("SkinnedMesh Renderer", _realPuppet.JawMeshRenderer, typeof(SkinnedMeshRenderer), true) as SkinnedMeshRenderer;

                    if (_realPuppet.JawMeshRenderer != null)
                    {
                        var options = new string[_realPuppet.JawMeshRenderer.sharedMesh.blendShapeCount];
                        for (var i = 0; i < options.Length; i++)
                        {
                            options[i] = i.ToString();
                        }
                        _realPuppet.JawBlendShapeIndex = EditorGUILayout.Popup("BlendShape Index", _realPuppet.JawBlendShapeIndex, options);
                    }
                    EditorGUILayout.EndVertical();
                }
                
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(10);
                if (Application.isPlaying)
                {
                    GUI.contentColor = Color.green;
                    EditorGUILayout.LabelField($"Input: {_realPuppet.JawGlove}");
                    GUI.contentColor = Color.white;
                }
                
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
                _realPuppet.JawSmoothness = EditorGUILayout.Slider("Smoothness", _realPuppet.JawSmoothness, 0, .3f);
            }
            EditorGUI.indentLevel--;
            
            
            // Limbs
            GUILayout.Space(10);
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUI.color = Color.yellow;
            GUILayout.Label("LIMBS", EditorStyles.whiteLargeLabel);
            GUI.color = Color.white;
            
            var limb = DropAreaGameObjectGUI("Drop Limb Root Bone Here".ToUpper());
            if (limb != null && limb.GetComponentInChildren<DynamicBone>() == null)
            {
                var dynamicBone = limb.AddComponent<DynamicBone>();
                dynamicBone.m_Root = limb.transform;
                _realPuppet.DynamicBones.Add(dynamicBone);
                Repaint();
            }
            
            GUILayout.Space(10);
            _dynamicBonesList.DoLayoutList();
            
            
            
            // Apply changes
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
        }
        
        private void CreateJointsList()
        {
            _jointsList = new ReorderableList(_realPuppet.PuppetJoints, typeof(RealPuppet.PuppetJoint), false, true, false, true);
            _jointsList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var defColor = GUI.color;
                
                var x = rect.x;
                rect.y += 2;
                var element = _jointsList.list[index] as RealPuppet.PuppetJoint;
                if (element == null)
                    return;
                
                element.Enabled = EditorGUI.Toggle(
                    new Rect(rect.x, rect.y, 35, EditorGUIUtility.singleLineHeight),
                    element.Enabled);
            
                GUI.color = element.RealPuppetDataProvider != null ? defColor : Color.red;
                rect.x += 35;
                element.RealPuppetDataProvider = EditorGUI.ObjectField(
                    new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight),
                    element.RealPuppetDataProvider,
                    typeof(RealPuppetDataProvider), true) as RealPuppetDataProvider;
                GUI.color = defColor;
            
                rect.x += 200;
                element.InputSource = (RealPuppetDataProvider.Source)EditorGUI.EnumPopup(
                    new Rect(rect.x, rect.y, rect.width - 235, EditorGUIUtility.singleLineHeight), 
                    element.InputSource);
            
                GUI.color = element.Joint != null ? defColor : Color.red;
                rect.x = x;
                rect.y += EditorGUIUtility.singleLineHeight * 1.25f;
                element.Joint = EditorGUI.ObjectField(
                    new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    "Joint",
                    element.Joint,
                    typeof(Transform), true) as Transform;
                GUI.color = defColor;

                rect.x = x;
                rect.y += EditorGUIUtility.singleLineHeight * 1.25f;
                element.Offset = EditorGUI.Vector3Field(
                    new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    "Offset",
                    element.Offset);
            
                rect.y += EditorGUIUtility.singleLineHeight * 1.25f;
                element.Sharpness = EditorGUI.Slider(
                    new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    "Sharpness",
                    element.Sharpness, 0, 1);

                if (Application.isPlaying && element.RealPuppetDataProvider != null)
                {
                    GUI.color = Color.green;
                    rect.y += EditorGUIUtility.singleLineHeight * 1.25f;
                    EditorGUI.LabelField(
                        new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        new GUIContent($"Input: {element.RealPuppetDataProvider.GetInput(element.InputSource).eulerAngles}"));
                    GUI.color = defColor;

                    var calibData = element.RealPuppetDataProvider.GetCalibrationData(element.InputSource);
                    GUI.color = calibData.IsCalibrated ? Color.green : Color.yellow;
                    rect.y += EditorGUIUtility.singleLineHeight * 1.25f;
                    EditorGUI.LabelField(
                        new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        new GUIContent($"System: {calibData.System}  Gyro: {calibData.Gyro}  Accl: {calibData.Accelerometer}  Mag:  {calibData.Magnetometer}"));
                    GUI.color = defColor;
                }
                
                // Divider
                if (index < _jointsList.count - 1)
                {
                    rect.x = x;
                    rect.y += EditorGUIUtility.singleLineHeight * 1.5f;
                    EditorGUI.TextArea(
                        new Rect(rect.x, rect.y, rect.width, 1),
                        "", 
                        GUI.skin.horizontalSlider);
                }
            };

            _jointsList.drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Joints"); };
            
            _jointsList.elementHeight = Application.isPlaying ? 140 : 100;
        }

        private void CreateDynamicBonesList()
        {
            _dynamicBonesList = new ReorderableList(_realPuppet.DynamicBones, typeof(DynamicBone), false, true, false, true);
            _dynamicBonesList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var x = rect.x;
                rect.y += 2;
                var element = _dynamicBonesList.list[index] as DynamicBone;
                if (element == null)
                    return;
                
                EditorGUI.BeginChangeCheck();
                element.m_Root = EditorGUI.ObjectField(
                    new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    "Root Bone", element.m_Root, typeof(Transform), true) as Transform;
            
                rect.y += EditorGUIUtility.singleLineHeight;
                element.m_Damping = EditorGUI.Slider(
                    new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    "Dampening",
                    element.m_Damping, 0, 1);
            
                rect.y += EditorGUIUtility.singleLineHeight;
                element.m_Elasticity = EditorGUI.Slider(
                    new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    "Elasticity",
                    element.m_Elasticity, 0, 1);
            
                rect.y += EditorGUIUtility.singleLineHeight;
                element.m_Stiffness = EditorGUI.Slider(
                    new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    "Stiffness",
                    element.m_Stiffness, 0, 1);

                if (EditorGUI.EndChangeCheck())
                {
                    element.UpdateParameters();
                }
                
                // Divider
                if (index < _dynamicBonesList.count - 1)
                {
                    rect.x = x;
                    rect.y += EditorGUIUtility.singleLineHeight * 1.5f;
                    EditorGUI.TextArea(rect, "", GUI.skin.horizontalSlider);
                }
            };

            _dynamicBonesList.drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Limbs"); };
            
            _dynamicBonesList.onRemoveCallback = list =>
            {
                DestroyImmediate(_realPuppet.DynamicBones[list.index]);
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
            };

            _dynamicBonesList.elementHeight = 90;
        }
        
        private static GameObject DropAreaGameObjectGUI(string message)
        {
            var evt = Event.current;
            var dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            var style = new GUIStyle("box");
            if (EditorGUIUtility.isProSkin)
                style.normal.textColor = Color.white;
            GUI.Box(dropArea, $"\n{message}", style);

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(evt.mousePosition))
                        return null;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (var draggedObject in DragAndDrop.objectReferences)
                        {
                            if (!(draggedObject is GameObject go)) continue;
                            
                            return go;
                        }
                    }
                    break;
            }

            return null;
        }
    }
}