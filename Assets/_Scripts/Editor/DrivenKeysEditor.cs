using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Thinko
{
    [CustomEditor(typeof(DrivenKeys))]
    public class DrivenKeysEditor : Editor
    {
        private class BlendShapeEditState
        {
            public float OriginalBlendShapeValue;
        }
        
        private class TransformEditState
        {
            public bool EditInitPosition;
            public bool EditEndPosition;
//            public bool EditInitRotation;
//            public bool EditEndRotation;
            public Vector3 OriginalPos;
        }

        private DrivenKeys _drivenKeys;

        private ReorderableList _blendshapesList;
        private List<BlendShapeEditState> _blendShapeEditStates;
        
        private ReorderableList _transformsList;
        private List<TransformEditState> _transformEditStates;

        private bool _editTransformMode;
        private bool _previewMode;
        private float _step;

        private void OnEnable()
        {
            _drivenKeys = target as DrivenKeys;

            // BlendShapes List
            CreateBlendShapesList();
            
            CreateBlendShapesEditState();


            // Transforms List
            CreateTransformsList();

            CreateTransformsEditState();
        }

        private void OnDisable()
        {
            for (var i = 0; i < _drivenKeys.BlendShapeKeys.Count; i++)
            {
                if (_drivenKeys.BlendShapeKeys[i].SkinnedMeshRenderer != null)
                    _drivenKeys.BlendShapeKeys[i].SkinnedMeshRenderer.SetBlendShapeWeight(_drivenKeys.BlendShapeKeys[i].BlendShapeIndex, _blendShapeEditStates[i].OriginalBlendShapeValue);
            }
            
            for (var i = 0; i < _drivenKeys.TransformKeys.Count; i++)
            {
                if (_drivenKeys.TransformKeys[i].Transform != null)
                    _drivenKeys.TransformKeys[i].Transform.localPosition = _transformEditStates[i].OriginalPos;
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            GUI.enabled = !_previewMode;

            // Add blendshape button
            if (GUILayout.Button("Add BlendShape"))
            {
                _drivenKeys.BlendShapeKeys.Add(new DrivenKeys.BlendShapeKey()
                {
                    SkinnedMeshRenderer = _drivenKeys.GetComponentInChildren<SkinnedMeshRenderer>()
                });
                _blendShapeEditStates.Add(new BlendShapeEditState());
            }

            // Blendshapes List
            _blendshapesList.DoLayoutList();


            // Add transform button
            EditorGUILayout.Space();
            if (GUILayout.Button("Add Transform"))
            {
                _drivenKeys.TransformKeys.Add(new DrivenKeys.TransformKey());
                _transformEditStates.Add(new TransformEditState());
            }

            // Transforms List
            _transformsList.DoLayoutList();
            
            // Edit transforms mode
            _editTransformMode = false;
            foreach (var transformEditState in _transformEditStates)
            {
                if (transformEditState.EditInitPosition || transformEditState.EditEndPosition)// || transformEditState.EditInitRotation || transformEditState.EditEndRotation)
                    _editTransformMode = true;
            }
            
            GUI.enabled = true;

            // Preview
            EditorGUILayout.Space();
            GUI.enabled = !_editTransformMode;
            GUI.color = _previewMode ? Color.green : Color.white;
            if (GUILayout.Button("PREVIEW"))
            {
                _previewMode = !_previewMode;

                if (_previewMode)
                    _drivenKeys.DoStep(_step);
                else
                    OnDisable();
            }
            GUI.enabled = true;
            GUI.color = Color.white;

            if (_previewMode)
            {
                EditorGUI.BeginChangeCheck();
                _step = EditorGUILayout.Slider(_step, 0, 1);
                if (EditorGUI.EndChangeCheck())
                {
                    _drivenKeys.DoStep(_step);
                }
            }

            // Apply changes
            serializedObject.ApplyModifiedProperties();
            if (!EditorGUI.EndChangeCheck()) return;
            Undo.RecordObject(_drivenKeys, "Modified DrivenKeys Component");
            EditorUtility.SetDirty(_drivenKeys);
        }

        private void OnSceneGUI()
        {
            if (!_drivenKeys.enabled)
                return;

            var count = 0;
            foreach (var transf in _drivenKeys.TransformKeys)
            {
                if (transf.Transform == null) continue;

                // Draw position handle 
                if (_transformEditStates[count].EditInitPosition || _transformEditStates[count].EditEndPosition)
                {
                    EditorGUI.BeginChangeCheck();
                    var newPos = Handles.PositionHandle(transf.Transform.position, transf.Transform.rotation);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(transf.Transform, "Move");
                        transf.Transform.position = newPos;
                    }
                }

                count++;
            }
        }

        private void EditPositionButton(Rect rect, ref Transform elementTransform, ref Vector3 elementPos, ref bool editPosition, ref Vector3 originalPosition)
        {
            GUI.enabled = (editPosition || !_editTransformMode) && !_previewMode;

            var defColor = GUI.color;
            GUI.color = editPosition ? Color.green : Color.white;
            
            if (GUI.Button(new Rect(
                    rect.x + rect.width - 100,
                    rect.y,
                    100,
                    EditorGUIUtility.singleLineHeight),
                "Edit"))
            {
                editPosition = !editPosition;
                if (editPosition)
                {
                    originalPosition = elementTransform.localPosition;
                    elementTransform.localPosition = elementPos;
                }
                else
                {
                    var newPos = elementTransform.localPosition;
                    elementTransform.localPosition = originalPosition;
                    elementPos = newPos;
                }
            }

            GUI.color = defColor;

            GUI.enabled = !_previewMode;
        }

        private void CreateBlendShapesList()
        {
            _blendshapesList = new ReorderableList(_drivenKeys.BlendShapeKeys, typeof(DrivenKeys.BlendShapeKey), false, true, false, true);
            _blendshapesList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var x = rect.x;
                rect.y += 2;
                var element = _blendshapesList.list[index] as DrivenKeys.BlendShapeKey;
                if (element == null)
                    return;

                // SkinnedMeshRenderer
                EditorGUI.BeginChangeCheck();
                element.SkinnedMeshRenderer = EditorGUI.ObjectField(new Rect(
                        rect.x,
                        rect.y,
                        rect.width,
                        EditorGUIUtility.singleLineHeight),
                    "Skinned Mesh Renderer",
                    element.SkinnedMeshRenderer,
                    typeof(SkinnedMeshRenderer),
                    true) as SkinnedMeshRenderer;
                
                // BlendShape values
                // Index
                rect.x = x;
                rect.y += EditorGUIUtility.singleLineHeight * 2f;
                EditorGUI.PrefixLabel(new Rect(
                        rect.x,
                        rect.y,
                        90,
                        EditorGUIUtility.singleLineHeight),
                    new GUIContent("BlendShape: Index", ""));
                rect.x += 110;
                var options = new string[element.SkinnedMeshRenderer.sharedMesh.blendShapeCount];
                for (var i = 0; i < options.Length; i++)
                {
                    options[i] = i.ToString();
                }
                element.BlendShapeIndex = EditorGUI.Popup(new Rect(
                        rect.x,
                        rect.y,
                        rect.width / 3 - 110,
                        EditorGUIUtility.singleLineHeight),
                    element.BlendShapeIndex,
                    options);
                
                
                // Setup values
                if (EditorGUI.EndChangeCheck())
                {
                    if (element.BlendShapeIndex >= element.SkinnedMeshRenderer.sharedMesh.blendShapeCount)
                        element.BlendShapeIndex = element.SkinnedMeshRenderer.sharedMesh.blendShapeCount - 1;
                    else if (element.BlendShapeIndex < 0)
                        element.BlendShapeIndex = 0;
                    
                    if(element.SkinnedMeshRenderer != null)
                        _blendShapeEditStates[index].OriginalBlendShapeValue = element.SkinnedMeshRenderer.GetBlendShapeWeight(element.BlendShapeIndex);
                }
                
                // Min
                rect.x = x + rect.width / 3 + 10;
                EditorGUI.PrefixLabel(new Rect(
                        rect.x,
                        rect.y,
                        30,
                        EditorGUIUtility.singleLineHeight),
                    new GUIContent("Min", ""));
                rect.x += 30;
                element.BlendShapeMin = EditorGUI.FloatField(new Rect(
                        rect.x,
                        rect.y,
                        rect.width / 3 - 30,
                        EditorGUIUtility.singleLineHeight),
                    element.BlendShapeMin);
                // Max
                rect.x = x + 2 * (rect.width / 3) + 20;
                EditorGUI.PrefixLabel(new Rect(
                        rect.x,
                        rect.y,
                        30,
                        EditorGUIUtility.singleLineHeight),
                    new GUIContent("Max", ""));
                rect.x += 30;
                element.BlendShapeMax = EditorGUI.FloatField(new Rect(
                        rect.x,
                        rect.y,
                        rect.width / 3 - 50,
                        EditorGUIUtility.singleLineHeight),
                    element.BlendShapeMax);
                
                // Divider
                if (index < _blendshapesList.count - 1)
                {
                    rect.x = x;
                    rect.y += EditorGUIUtility.singleLineHeight * 2f;
                    EditorGUI.TextArea(rect, "", GUI.skin.horizontalSlider);
                }
            };

            _blendshapesList.drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "BlendShapes"); };
            
            _blendshapesList.onRemoveCallback = list =>
            {
                _blendShapeEditStates.RemoveAt(list.index);
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
            };

            _blendshapesList.elementHeight = 90;
        }

        private void CreateBlendShapesEditState()
        {
            _blendShapeEditStates = new List<BlendShapeEditState>();
            foreach (var bk in _drivenKeys.BlendShapeKeys)
            {
                if (bk.SkinnedMeshRenderer == null) continue;
                _blendShapeEditStates.Add(new BlendShapeEditState()
                {
                    OriginalBlendShapeValue = bk.SkinnedMeshRenderer.GetBlendShapeWeight(bk.BlendShapeIndex)
                });
            }
        }

        private void CreateTransformsList()
        {
            _transformsList = new ReorderableList(_drivenKeys.TransformKeys, typeof(DrivenKeys.TransformKey), false, true, false, true);
            _transformsList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var x = rect.x;
                rect.y += 2;
                var element = _transformsList.list[index] as DrivenKeys.TransformKey;
                if (element == null)
                    return;

                // Transform field
                EditorGUI.BeginChangeCheck();
                element.Transform = EditorGUI.ObjectField(new Rect(
                        rect.x,
                        rect.y,
                        rect.width,
                        EditorGUIUtility.singleLineHeight),
                    "Transform",
                    element.Transform,
                    typeof(Transform),
                    true) as Transform;
                if (EditorGUI.EndChangeCheck())
                {
                    // Setup start values
                    Undo.RecordObject(element.Transform, "Transform set");
                    element.InitPosition = element.Transform.localPosition;
                    element.EndPosition = element.Transform.localPosition;
                    _transformEditStates[index].OriginalPos = element.Transform.localPosition;
                }

                if (element.Transform == null)
                    return;

                // Start Position
                rect.x = x;
                rect.y += EditorGUIUtility.singleLineHeight * 1.5f;
                element.InitPosition = EditorGUI.Vector3Field(new Rect(
                        rect.x,
                        rect.y,
                        rect.width - 110,
                        EditorGUIUtility.singleLineHeight),
                    "Start Position",
                    element.InitPosition);

                EditPositionButton(rect, ref element.Transform, ref element.InitPosition, ref _transformEditStates[index].EditInitPosition, ref _transformEditStates[index].OriginalPos);

                // End Position
                rect.x = x;
                rect.y += EditorGUIUtility.singleLineHeight * 1.5f;
                element.EndPosition = EditorGUI.Vector3Field(new Rect(
                        rect.x,
                        rect.y,
                        rect.width - 110,
                        EditorGUIUtility.singleLineHeight),
                    "End Position",
                    element.EndPosition);

                EditPositionButton(rect, ref element.Transform, ref element.EndPosition, ref _transformEditStates[index].EditEndPosition, ref _transformEditStates[index].OriginalPos);
                
                // Divider
                if (index < _transformsList.count - 1)
                {
                    rect.x = x;
                    rect.y += EditorGUIUtility.singleLineHeight * 2f;
                    EditorGUI.TextArea(rect, "", GUI.skin.horizontalSlider);
                }
            };

            _transformsList.drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Transforms"); };

            _transformsList.onRemoveCallback = list =>
            {
                _transformEditStates.RemoveAt(list.index);
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
            };

            _transformsList.elementHeight = 110;
        }

        private void CreateTransformsEditState()
        {
            _transformEditStates = new List<TransformEditState>();
            foreach (var tk in _drivenKeys.TransformKeys)
            {
                if (tk.Transform == null) continue;
                _transformEditStates.Add(new TransformEditState()
                {
                    OriginalPos = tk.Transform.localPosition
                });
            }
        }
    }
}