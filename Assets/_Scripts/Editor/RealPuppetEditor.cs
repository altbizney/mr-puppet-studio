using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Thinko
{
    [CustomEditor(typeof(RealPuppet))]
    public class RealPuppetEditor : Editor
    {
        private class PuppetJawAnimDataEdit
        {
            public Vector3 OriginalPosition;
            public Quaternion OriginalRotation;
            public bool EditOpenPose;
            public bool EditClosePose;
        }
        
        private RealPuppet _realPuppet;

        private Transform[] _childNodes;

        private ReorderableList _jointsList;
        private ReorderableList _dynamicBonesList;

        private static float _playModeJawMin;
        private static float _playModeJawMax;

        private PuppetJawAnimDataEdit _jawAnimEdit;
        private bool _editJawMode;
        private bool _previewJawMode;
        private float _jawStep;

        private SerializedProperty _shoulderJointProperty;
        private SerializedProperty _shoulderOffsetProperty;
        private SerializedProperty _elbowJointProperty;
        private SerializedProperty _elbowOffsetProperty;
        private SerializedProperty _wristJointProperty;
        private SerializedProperty _wristOffsetProperty;

        private void OnEnable()
        {
            _realPuppet = target as RealPuppet;

            _shoulderJointProperty = serializedObject.FindProperty("ShoulderJoint");
            _shoulderOffsetProperty = serializedObject.FindProperty("ShoulderOffset");
            _elbowJointProperty = serializedObject.FindProperty("ElbowJoint");
            _elbowOffsetProperty = serializedObject.FindProperty("ElbowOffset");
            _wristJointProperty = serializedObject.FindProperty("WristJoint");
            _wristOffsetProperty = serializedObject.FindProperty("WristOffset");

            if (_realPuppet.RealBody == null)
                _realPuppet.RealBody = FindObjectOfType<RealBody>();
            
            if(_realPuppet.JawRealPuppetDataProvider == null)
                _realPuppet.JawRealPuppetDataProvider = FindObjectOfType<RealPuppetDataProvider>();

            _realPuppet.DynamicBones = _realPuppet.GetComponentsInChildren<DynamicBone>().ToList();
            CreateDynamicBonesList();
            
            if (_realPuppet.JawNode != null)
            {
                CreateJawEdit();
            }
        }

        private void OnDisable()
        {
            DisableJawEdit();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            serializedObject.Update();

            GUILayout.Space(10);

            var defColor = GUI.color;
            
            // Joints
            GUILayout.Space(10);
            GUI.color = Color.yellow;
            GUILayout.Label("JOINTS", EditorStyles.whiteLargeLabel);
            GUI.color = defColor;
            
            GUI.color = _realPuppet.RealBody != null ? defColor : Color.red;
            _realPuppet.RealBody = EditorGUILayout.ObjectField("RealBody", _realPuppet.RealBody, typeof(RealBody), true) as RealBody;
            GUI.color = defColor;
            GUILayout.Space(10);
            
            if (_realPuppet.RealBody != null)
            {
                JointGUI(_shoulderJointProperty, _shoulderOffsetProperty);
                JointGUI(_elbowJointProperty, _elbowOffsetProperty);
                JointGUI(_wristJointProperty, _wristOffsetProperty);
            }

            // Jaw
            GUILayout.Space(10);
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUI.color = Color.yellow;
            GUILayout.Label("JAW", EditorStyles.whiteLargeLabel);
            GUI.color = defColor;

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
                    EditorGUI.BeginChangeCheck();
                    _realPuppet.JawNode = EditorGUILayout.ObjectField("Joint", _realPuppet.JawNode, typeof(Transform), true) as Transform;
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (_realPuppet.JawNode != null)
                        {
                            CreateJawEdit();
                        }
                    }
                    
                    // Edit jaw
                    EditorGUILayout.BeginHorizontal();
                    if (_realPuppet.JawNode != null && _jawAnimEdit != null)
                    {
                        _editJawMode = _jawAnimEdit.EditOpenPose || _jawAnimEdit.EditClosePose;
                        EditorGUILayout.BeginVertical();
                        EditTransformButton("Edit Open Pose", ref _realPuppet.JawNode, ref _realPuppet.JawAnimData.OpenPosition, ref _realPuppet.JawAnimData.OpenRotation, ref _jawAnimEdit.EditOpenPose, ref _jawAnimEdit.OriginalPosition, ref _jawAnimEdit.OriginalRotation);
                        EditTransformButton("Edit Close Pose", ref _realPuppet.JawNode, ref _realPuppet.JawAnimData.ClosePosition, ref _realPuppet.JawAnimData.CloseRotation, ref _jawAnimEdit.EditClosePose, ref _jawAnimEdit.OriginalPosition, ref _jawAnimEdit.OriginalRotation);
                        EditorGUILayout.EndVertical();
                        // Preview
                        GUI.enabled = !_editJawMode && !Application.isPlaying;
                        if (_previewJawMode)
                        {
                            EditorGUI.BeginChangeCheck();
                            _jawStep = EditorGUILayout.Slider(_jawStep, 0, 1);
                            if (EditorGUI.EndChangeCheck())
                            {
                                JawStep(_jawStep);
                            }
                        }
                        GUI.color = _previewJawMode ? Color.green : Color.white;
                        if (GUILayout.Button("PREVIEW", GUILayout.ExpandHeight(true)))
                        {
                            _previewJawMode = !_previewJawMode;

                            if (_previewJawMode)
                                JawStep(_jawStep);
                            else
                                DisableJawEdit();
                        }
                        GUI.enabled = true;
                        GUI.color = Color.white;

                        
                    }
                    EditorGUILayout.EndHorizontal();
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
                _realPuppet.JawSmoothness = EditorGUILayout.Slider("Smoothness", _realPuppet.JawSmoothness, 0, .3f);
            }
            EditorGUI.indentLevel--;


            // Limbs
            GUILayout.Space(10);
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            GUI.color = Color.yellow;
            GUILayout.Label("LIMBS", EditorStyles.whiteLargeLabel);
            GUI.color = defColor;

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

        private void JointGUI(SerializedProperty jointProperty, SerializedProperty jointOffsetProperty)
        {
            EditorGUILayout.PropertyField(jointProperty);
            if (jointProperty.objectReferenceValue == null) return;
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(jointOffsetProperty);
            EditorGUI.indentLevel--;
        }
        
        private void OnSceneGUI()
        {
            if (!_realPuppet.enabled)
                return;

            // Draw jaw transform handle 
            if (_jawAnimEdit != null && (_jawAnimEdit.EditOpenPose || _jawAnimEdit.EditClosePose))
            {
                EditorGUI.BeginChangeCheck();
                var pos = _realPuppet.JawNode.position;
                var rot = _realPuppet.JawNode.rotation;
                Handles.TransformHandle(ref pos, ref rot);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_realPuppet.JawNode, "Adjust jaw");
                    _realPuppet.JawNode.position = pos;
                    _realPuppet.JawNode.rotation = rot;
                }
            }
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
                    EditorGUI.TextArea(
                        new Rect(rect.x, rect.y, rect.width, 1),
                        "",
                        GUI.skin.horizontalSlider);
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
        
        private void EditTransformButton(string button, ref Transform transf, ref Vector3 pos, ref Quaternion rot, ref bool edit, ref Vector3 originalPosition, ref Quaternion originalRotation)
        {
            GUI.enabled = (edit || !_editJawMode) && !_previewJawMode && !Application.isPlaying;

            var defColor = GUI.color;
            GUI.color = edit ? Color.green : Color.white;
            
            if (GUILayout.Button(button))
            {
                edit = !edit;
                if (edit)
                {
                    originalPosition = transf.localPosition;
                    originalRotation = transf.localRotation;
                    transf.localPosition = pos;
                    transf.localRotation = rot;
                    Tools.hidden = true; // Hides the default gizmos so they don't get in the way
                }
                else
                {
                    var newPos = transf.localPosition;
                    transf.localPosition = originalPosition;
                    pos = newPos;
                    var newRot = transf.localRotation;
                    transf.localRotation = originalRotation;
                    rot = newRot;
                    Tools.hidden = false;
                }
            }

            GUI.color = defColor;

            GUI.enabled = !_previewJawMode;
        }

        private void CreateJawEdit()
        {
            _jawAnimEdit = new PuppetJawAnimDataEdit()
            {
                OriginalPosition = _realPuppet.JawNode.localPosition,
                OriginalRotation = _realPuppet.JawNode.localRotation,
            };

            if (_realPuppet.JawAnimData.ClosePosition == Vector3.zero &&
                _realPuppet.JawAnimData.CloseRotation == Quaternion.identity && 
                _realPuppet.JawAnimData.OpenPosition == Vector3.zero && 
                _realPuppet.JawAnimData.OpenRotation == Quaternion.identity)
            {
                _realPuppet.JawAnimData = new RealPuppet.PuppetJawAnimData()
                {
                    OpenPosition = _realPuppet.JawNode.localPosition,
                    OpenRotation = _realPuppet.JawNode.localRotation,
                    ClosePosition = _realPuppet.JawNode.localPosition,
                    CloseRotation = _realPuppet.JawNode.localRotation
                };
            }
        }
        
        private void DisableJawEdit()
        {
            if (_realPuppet.JawNode != null)
            {
                _realPuppet.JawNode.localPosition = _jawAnimEdit.OriginalPosition;
                _realPuppet.JawNode.localRotation = _jawAnimEdit.OriginalRotation;
            }
        }
        
        private void JawStep(float step)
        {
            _realPuppet.JawNode.localPosition = Vector3.Lerp(_realPuppet.JawAnimData.OpenPosition, _realPuppet.JawAnimData.ClosePosition, step);
            _realPuppet.JawNode.localRotation = Quaternion.Lerp(_realPuppet.JawAnimData.OpenRotation, _realPuppet.JawAnimData.CloseRotation, step);
        }
    }
}