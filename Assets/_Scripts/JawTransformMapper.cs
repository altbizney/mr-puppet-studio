using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace MrPuppet
{
    public class JawTransformMapper : MonoBehaviour
    {
        [Serializable]
        public class JawAnimData
        {
            public Vector3 OpenPosition = Vector3.zero;
            public Vector3 OpenMaxPosition = Vector3.zero;
            public Quaternion OpenRotation = Quaternion.identity;
            public Quaternion OpenMaxRotation = Quaternion.identity;
            public Vector3 ClosePosition = Vector3.zero;
            public Vector3 CloseMaxPosition = Vector3.zero;
            public Quaternion CloseRotation = Quaternion.identity;
            public Quaternion CloseMaxRotation = Quaternion.identity;
        }

        [Required]
        public MrPuppetDataMapper DataMapper;

        [Required]
        [OnValueChanged(nameof(OnJawJointAssigned))]
        public Transform JawJoint;

        [HideInInspector]
        public JawAnimData AnimData;

        [Range(0f, 0.5f)]
        public float SmoothTime = 0.02f;

        private float _jawPercentSmoothed;
        private float _jawPercentVelocity;
        private Vector3 _jawCurrentVelocity;

        private void OnValidate()
        {
            if (DataMapper == null) DataMapper = FindObjectOfType<MrPuppetDataMapper>();
        }

        private void Update()
        {
            // TODO: should JawPercent be smoothed, and/or position and rotation?

            JawJoint.localPosition = Vector3.SmoothDamp(
                JawJoint.localPosition,
                Vector3.Lerp(AnimData.ClosePosition, AnimData.OpenPosition, DataMapper.JawPercent),
                ref _jawCurrentVelocity,
                SmoothTime);

            _jawPercentSmoothed = Mathf.SmoothDamp(_jawPercentSmoothed, DataMapper.JawPercent, ref _jawPercentVelocity, SmoothTime);
            JawJoint.localRotation = Quaternion.Slerp(
                AnimData.CloseRotation,
                AnimData.OpenRotation,
                _jawPercentSmoothed);

            // _jawPercentSmoothed = Mathf.SmoothDamp(_jawPercentSmoothed, DataMapper.JawPercent, ref _jawPercentVelocity, SmoothTime);
            // DebugGraph.Log(Quaternion.Slerp(AnimData.CloseRotation, AnimData.OpenRotation, DataMapper.JawPercent));
            // DebugGraph.Log(Quaternion.SlerpUnclamped(AnimData.CloseRotation, AnimData.OpenRotation, DataMapper.JawPercent));
            // DebugGraph.Log(Quaternion.Angle(
            //     Quaternion.Slerp(AnimData.CloseRotation, AnimData.OpenRotation, DataMapper.JawPercent),
            //     Quaternion.SlerpUnclamped(AnimData.CloseRotation, AnimData.OpenRotation, DataMapper.JawPercent)
            // ));
            // JawJoint.localRotation = Quaternion.SlerpUnclamped(AnimData.CloseRotation, AnimData.OpenRotation, DataMapper.JawPercent);
        }

        private void OnJawJointAssigned()
        {
            if (JawJoint == null) return;
            AnimData = new JawAnimData()
            {
                OpenPosition = JawJoint.localPosition,
                OpenMaxPosition = JawJoint.localPosition,
                OpenRotation = JawJoint.localRotation,
                OpenMaxRotation = JawJoint.localRotation,
                ClosePosition = JawJoint.localPosition,
                CloseMaxPosition = JawJoint.localPosition,
                CloseRotation = JawJoint.localRotation,
                CloseMaxRotation = JawJoint.localRotation
            };
        }

        // The section below is used to store the changes made at runtime
        static JawTransformMapper()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private const string JawSmoothTimeKey = "jawSmoothTime";

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            var jawTransformMapper = FindObjectOfType<JawTransformMapper>();
            if (jawTransformMapper == null) return;

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                Undo.RecordObject(jawTransformMapper, "Undo JawTransformMapper");
                jawTransformMapper.SmoothTime = PlayerPrefs.GetFloat(JawSmoothTimeKey);
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                PlayerPrefs.SetFloat(JawSmoothTimeKey, jawTransformMapper.SmoothTime);
            }
        }
    }

    // Editor
    [CustomEditor(typeof(JawTransformMapper))]
    public class JawTransformMapperEditor : OdinEditor
    {
        private class JawAnimDataEdit
        {
            public Vector3 OriginalPosition;
            public Quaternion OriginalRotation;
            public bool EditOpenPose;
            public bool EditOpenMax;
            public bool EditClosePose;
            public bool EditCloseMax;
        }

        private JawAnimDataEdit _jawAnimDataEdit;
        private JawTransformMapper _jawTransformMapper;
        private bool _editJawMode;
        private bool _previewJawMode;
        private float _jawStep;

        protected override void OnEnable()
        {
            base.OnEnable();

            _jawTransformMapper = target as JawTransformMapper;

            _jawAnimDataEdit = new JawAnimDataEdit();
            if (_jawTransformMapper.JawJoint != null)
            {
                _jawAnimDataEdit.OriginalPosition = _jawTransformMapper.JawJoint.localPosition;
                _jawAnimDataEdit.OriginalRotation = _jawTransformMapper.JawJoint.localRotation;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (_editJawMode || _previewJawMode)
                ResetJawTransform();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            DrawDefaultInspector();

            if (_jawTransformMapper.JawJoint == null || _jawAnimDataEdit == null) return;

            _editJawMode = _jawAnimDataEdit.EditOpenMax || _jawAnimDataEdit.EditOpenPose || _jawAnimDataEdit.EditClosePose || _jawAnimDataEdit.EditCloseMax;

            // Edit buttons
            EditorGUILayout.BeginHorizontal();
            EditTransformButton("Edit Open Max", ref _jawTransformMapper.JawJoint, ref _jawTransformMapper.AnimData.OpenMaxPosition, ref _jawTransformMapper.AnimData.OpenMaxRotation, ref _jawAnimDataEdit.EditOpenMax, ref _jawAnimDataEdit.OriginalPosition, ref _jawAnimDataEdit.OriginalRotation);
            EditTransformButton("Edit Open Pose", ref _jawTransformMapper.JawJoint, ref _jawTransformMapper.AnimData.OpenPosition, ref _jawTransformMapper.AnimData.OpenRotation, ref _jawAnimDataEdit.EditOpenPose, ref _jawAnimDataEdit.OriginalPosition, ref _jawAnimDataEdit.OriginalRotation);
            EditTransformButton("Edit Close Pose", ref _jawTransformMapper.JawJoint, ref _jawTransformMapper.AnimData.ClosePosition, ref _jawTransformMapper.AnimData.CloseRotation, ref _jawAnimDataEdit.EditClosePose, ref _jawAnimDataEdit.OriginalPosition, ref _jawAnimDataEdit.OriginalRotation);
            EditTransformButton("Edit Close Max", ref _jawTransformMapper.JawJoint, ref _jawTransformMapper.AnimData.CloseMaxPosition, ref _jawTransformMapper.AnimData.CloseMaxRotation, ref _jawAnimDataEdit.EditCloseMax, ref _jawAnimDataEdit.OriginalPosition, ref _jawAnimDataEdit.OriginalRotation);
            EditorGUILayout.EndHorizontal();

            // Preview
            GUI.enabled = !_editJawMode && !Application.isPlaying;
            GUI.color = _previewJawMode ? Color.green : Color.white;
            if (GUILayout.Button("PREVIEW", GUILayout.ExpandHeight(true)))
            {
                _previewJawMode = !_previewJawMode;

                if (_previewJawMode)
                    JawStep(_jawStep);
                else
                    ResetJawTransform();
            }
            GUI.color = Color.white;

            if (_previewJawMode)
            {
                EditorGUI.BeginChangeCheck();
                _jawStep = EditorGUILayout.Slider(_jawStep, 0, 1);
                if (EditorGUI.EndChangeCheck())
                    JawStep(_jawStep);
            }

            // Apply changes
            serializedObject.ApplyModifiedProperties();
            if (!EditorGUI.EndChangeCheck()) return;
            Undo.RecordObject(_jawTransformMapper, "Modified JawTransformMapper Component");
            EditorUtility.SetDirty(_jawTransformMapper);
        }

        private void OnSceneGUI()
        {
            if (!_jawTransformMapper.enabled)
                return;

            // Draw jaw transform handle
            if (_jawAnimDataEdit != null && (_jawAnimDataEdit.EditOpenMax || _jawAnimDataEdit.EditOpenPose || _jawAnimDataEdit.EditClosePose || _jawAnimDataEdit.EditCloseMax))
            {
                EditorGUI.BeginChangeCheck();
                var pos = _jawTransformMapper.JawJoint.position;
                var rot = _jawTransformMapper.JawJoint.rotation;
                Handles.TransformHandle(ref pos, ref rot);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_jawTransformMapper.JawJoint, "Adjust jaw");
                    _jawTransformMapper.JawJoint.position = pos;
                    _jawTransformMapper.JawJoint.rotation = rot;
                }
            }
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
                    _jawAnimDataEdit.OriginalPosition = _jawTransformMapper.JawJoint.localPosition;
                    _jawAnimDataEdit.OriginalRotation = _jawTransformMapper.JawJoint.localRotation;

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

        private void ResetJawTransform()
        {
            if (_jawTransformMapper != null && _jawTransformMapper.JawJoint != null && _jawAnimDataEdit != null)
            {
                _jawTransformMapper.JawJoint.localPosition = _jawAnimDataEdit.OriginalPosition;
                _jawTransformMapper.JawJoint.localRotation = _jawAnimDataEdit.OriginalRotation;
            }
        }

        private void JawStep(float step)
        {
            _jawTransformMapper.JawJoint.localPosition = Vector3.Lerp(_jawTransformMapper.AnimData.OpenPosition, _jawTransformMapper.AnimData.ClosePosition, step);
            _jawTransformMapper.JawJoint.localRotation = Quaternion.Lerp(_jawTransformMapper.AnimData.OpenRotation, _jawTransformMapper.AnimData.CloseRotation, step);
        }
    }
}