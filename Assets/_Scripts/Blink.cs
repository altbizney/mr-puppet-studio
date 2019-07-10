using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MrPuppet
{
    public class Blink : MonoBehaviour
    {
        [Serializable]
        public class BlendShapeKey
        {
            public SkinnedMeshRenderer SkinnedMeshRenderer;
            public int BlendShapeIndex = 0;
            public float BlendShapeMin = 0;
            public float BlendShapeMax = 100;
        }

        [Serializable]
        public class TransformKey
        {
            public Transform Transform;
            public Vector3 InitPosition;
            public Vector3 EndPosition;
            public Quaternion InitRotation = Quaternion.identity;
            public Quaternion EndRotation = Quaternion.identity;
        }

        public bool AutoBlink = true;

        [MinMaxSlider(.01f, 10f)] public Vector2 BlinkInterval = new Vector2(1, 3);

        [MinMaxSlider(.01f, .2f)] public Vector2 BlinkCloseDuration = new Vector2(.05f, .1f);

        [MinMaxSlider(.01f, .2f)] public Vector2 BlinkOpenDuration = new Vector2(.05f, .1f);

        [MinMaxSlider(.01f, .2f)] public Vector2 BlinkHoldDuration = new Vector2(.05f, .1f);

        public bool Invert;

        public KeyCode ManualBlinkKey = KeyCode.B;

        [HideInInspector] public float Step = 1;

        public List<BlendShapeKey> BlendShapeKeys = new List<BlendShapeKey>();
        public List<TransformKey> TransformKeys = new List<TransformKey>();

        private float OpenedValue => Invert ? 1 : 0;
        private float ClosedValue => Invert ? 0 : 1;

        private float _previousStep;

        private void OnEnable()
        {
            if (AutoBlink)
            {
                Step = OpenedValue;
                StartCoroutine(BlinkRoutine());
            }
        }

        protected virtual void Update()
        {
            if (Input.GetKeyDown(ManualBlinkKey))
            {
                StopAllCoroutines();
                StartCoroutine(TweenEyelidRoutine(Random.Range(BlinkCloseDuration.x, BlinkCloseDuration.y), ClosedValue));
            }

            if (Input.GetKeyUp(ManualBlinkKey))
            {
                StartCoroutine(ManualOpenRoutine());
            }

            if (Math.Abs(Step - _previousStep) > float.Epsilon)
            {
                _previousStep = Step;
                DoStep(Step);
            }
        }

        public void DoStep(float step)
        {
            foreach (var blendShapeKey in BlendShapeKeys)
            {
                if (blendShapeKey.BlendShapeIndex < blendShapeKey.SkinnedMeshRenderer.sharedMesh.blendShapeCount)
                    blendShapeKey.SkinnedMeshRenderer.SetBlendShapeWeight(blendShapeKey.BlendShapeIndex, step.Remap(0, 1, blendShapeKey.BlendShapeMin, blendShapeKey.BlendShapeMax));
            }

            foreach (var transformKey in TransformKeys)
            {
                if (transformKey.Transform == null) continue;
                transformKey.Transform.localPosition = Vector3.Lerp(transformKey.InitPosition, transformKey.EndPosition, step);
                transformKey.Transform.localRotation = Quaternion.Lerp(transformKey.InitRotation, transformKey.EndRotation, step);
            }
        }

        private IEnumerator BlinkRoutine()
        {
            while (AutoBlink)
            {
                yield return new WaitForSeconds(Random.Range(BlinkInterval.x, BlinkInterval.y));

                yield return StartCoroutine(TweenEyelidRoutine(Random.Range(BlinkCloseDuration.x, BlinkCloseDuration.y), ClosedValue));

                yield return new WaitForSeconds(Random.Range(BlinkHoldDuration.x, BlinkHoldDuration.y));

                yield return StartCoroutine(TweenEyelidRoutine(Random.Range(BlinkOpenDuration.x, BlinkOpenDuration.y), OpenedValue));
            }
        }

        private IEnumerator TweenEyelidRoutine(float duration, float targetValue)
        {
            var t = 0f;
            while (t <= 1f)
            {
                t += Time.deltaTime / duration;

                Step = Mathf.Lerp(Step, targetValue, t);

                yield return null;
            }
        }

        private IEnumerator ManualOpenRoutine()
        {
            yield return StartCoroutine(TweenEyelidRoutine(Random.Range(BlinkOpenDuration.x, BlinkOpenDuration.y), OpenedValue));

            if (AutoBlink)
                StartCoroutine(BlinkRoutine());
        }
    }

    [CustomEditor(typeof(Blink))]
    public class BlinkEditor : Editor
    {
        private class BlendShapeEditState
        {
            public float OriginalBlendShapeValue;
        }

        private class TransformEditState
        {
            public bool EditInit;
            public bool EditEnd;
            public Vector3 OriginalPosition;
            public Quaternion OriginalRotation;
        }

        private Blink _blink;

        private ReorderableList _blendshapesList;
        private List<BlendShapeEditState> _blendShapeEditStates;

        private ReorderableList _transformsList;

        private List<TransformEditState> _transformEditStates;
        private bool _editTransformMode;
        private bool _previewMode;
        private float _step;

        private void OnEnable()
        {
            _blink = target as Blink;

            // BlendShapes List
            CreateBlendShapesList();

            CreateBlendShapesEditState();


            // Transforms List
            CreateTransformsList();

            CreateTransformsEditState();
        }

        private void OnDisable()
        {
            for (var i = 0; i < _blink.BlendShapeKeys.Count; i++)
            {
                if (_blink.BlendShapeKeys[i].SkinnedMeshRenderer != null)
                    _blink.BlendShapeKeys[i].SkinnedMeshRenderer.SetBlendShapeWeight(_blink.BlendShapeKeys[i].BlendShapeIndex, _blendShapeEditStates[i].OriginalBlendShapeValue);
            }

            for (var i = 0; i < _blink.TransformKeys.Count; i++)
            {
                if (_blink.TransformKeys[i].Transform != null)
                {
                    _blink.TransformKeys[i].Transform.localPosition = _transformEditStates[i].OriginalPosition;
                    _blink.TransformKeys[i].Transform.localRotation = _transformEditStates[i].OriginalRotation;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            DrawDefaultInspector();

            GUI.enabled = !_previewMode;

            EditorGUILayout.Space();

            // Blendshapes List
            _blendshapesList.DoLayoutList();

            EditorGUILayout.Space();

            // Transforms List
            _transformsList.DoLayoutList();

            // Edit transforms mode
            _editTransformMode = false;
            foreach (var transformEditState in _transformEditStates)
            {
                if (transformEditState.EditInit || transformEditState.EditEnd)
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
                    _blink.DoStep(_step);
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
                    _blink.DoStep(_step);
                }
            }

            // Apply changes
            serializedObject.ApplyModifiedProperties();
            if (!EditorGUI.EndChangeCheck()) return;
            Undo.RecordObject(_blink, "Modified Blink Component");
            EditorUtility.SetDirty(_blink);
        }

        private void OnSceneGUI()
        {
            if (!_blink.enabled)
                return;

            var count = 0;
            foreach (var transf in _blink.TransformKeys)
            {
                if (transf.Transform == null) continue;

                // Draw position handle
                if (_transformEditStates[count].EditInit || _transformEditStates[count].EditEnd)
                {
                    EditorGUI.BeginChangeCheck();
                    var pos = transf.Transform.position;
                    var rot = transf.Transform.rotation;
                    Handles.TransformHandle(ref pos, ref rot);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(transf.Transform, "Move");
                        transf.Transform.position = pos;
                        transf.Transform.rotation = rot;
                    }
                }

                count++;
            }
        }

        private void EditPositionButton(string buttonLabel, Rect rect, ref Transform transf, ref Vector3 pos, ref Quaternion rot, ref bool edit, ref Vector3 originalPosition, ref Quaternion originalRotation)
        {
            GUI.enabled = (edit || !_editTransformMode) && !_previewMode;

            var defColor = GUI.color;
            GUI.color = edit ? Color.green : Color.white;

            if (GUI.Button(new Rect(
                    rect.x + rect.width - 100,
                    rect.y,
                    100,
                    EditorGUIUtility.singleLineHeight),
                buttonLabel))
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

            GUI.enabled = !_previewMode;
        }

        private void CreateBlendShapesList()
        {
            _blendshapesList = new ReorderableList(_blink.BlendShapeKeys, typeof(Blink.BlendShapeKey), false, true, true, true);
            _blendshapesList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var x = rect.x;
                rect.y += 2;
                var element = _blendshapesList.list[index] as Blink.BlendShapeKey;
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

                if(element.SkinnedMeshRenderer == null)
                    return;

                // BlendShape values
                // Index
                rect.x = x;
                rect.y += EditorGUIUtility.singleLineHeight * 2f;
                EditorGUI.PrefixLabel(new Rect(
                        rect.x,
                        rect.y,
                        90,
                        EditorGUIUtility.singleLineHeight),
                    new GUIContent("BlendShape:", ""));
                rect.x += 110;
                var options = new string[element.SkinnedMeshRenderer.sharedMesh.blendShapeCount];
                for (var i = 0; i < options.Length; i++)
                {
                    options[i] = element.SkinnedMeshRenderer.sharedMesh.GetBlendShapeName(i);
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

                    if (element.SkinnedMeshRenderer != null)
                    {
                        if (_blendShapeEditStates[index] == null)
                        {
                            _blendShapeEditStates.Add(new BlendShapeEditState()
                            {
                                OriginalBlendShapeValue = element.SkinnedMeshRenderer.GetBlendShapeWeight(element.BlendShapeIndex)
                            });
                        }
                        _blendShapeEditStates[index].OriginalBlendShapeValue = element.SkinnedMeshRenderer.GetBlendShapeWeight(element.BlendShapeIndex);
                    }
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

            _blendshapesList.onAddCallback = list =>
            {
                _blink.BlendShapeKeys.Add(new Blink.BlendShapeKey()
                {
                    SkinnedMeshRenderer = _blink.GetComponentInChildren<SkinnedMeshRenderer>()
                });
                _blendShapeEditStates.Add(new BlendShapeEditState());
            };

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
            foreach (var bk in _blink.BlendShapeKeys)
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
            _transformsList = new ReorderableList(_blink.TransformKeys, typeof(Blink.TransformKey), false, true, true, true);
            _transformsList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var x = rect.x;
                rect.y += 2;
                var element = _transformsList.list[index] as Blink.TransformKey;
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
                    element.InitRotation = element.Transform.localRotation;
                    element.EndRotation = element.Transform.localRotation;
                    _transformEditStates[index].OriginalPosition = element.Transform.localPosition;
                    _transformEditStates[index].OriginalRotation = element.Transform.localRotation;
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

                EditPositionButton("Edit", rect, ref element.Transform, ref element.InitPosition, ref element.InitRotation, ref _transformEditStates[index].EditInit, ref _transformEditStates[index].OriginalPosition, ref _transformEditStates[index].OriginalRotation);

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

                EditPositionButton("Edit", rect, ref element.Transform, ref element.EndPosition, ref element.EndRotation, ref _transformEditStates[index].EditEnd, ref _transformEditStates[index].OriginalPosition, ref _transformEditStates[index].OriginalRotation);

                // Divider
                if (index < _transformsList.count - 1)
                {
                    rect.x = x;
                    rect.y += EditorGUIUtility.singleLineHeight * 2f;
                    EditorGUI.TextArea(rect, "", GUI.skin.horizontalSlider);
                }
            };

            _transformsList.drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Transforms"); };

            _transformsList.onAddCallback = list =>
            {
                _blink.TransformKeys.Add(new Blink.TransformKey());
                _transformEditStates.Add(new TransformEditState());
            };

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
            foreach (var tk in _blink.TransformKeys)
            {
                if (tk.Transform == null) continue;
                _transformEditStates.Add(new TransformEditState()
                {
                    OriginalPosition = tk.Transform.localPosition,
                    OriginalRotation = tk.Transform.localRotation
                });
            }
        }
    }
}