using System;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif

namespace MrPuppet
{
    public class JawTransformMapper : MonoBehaviour
    {
        [Serializable]
        public class JawAnimData
        {
            public Quaternion OpenRotation = Quaternion.identity;
            public Quaternion CloseRotation = Quaternion.identity;
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

        // [HideInInspector]
        public bool ApplySensors = true;

        private void OnValidate()
        {
            if (DataMapper == null) DataMapper = FindObjectOfType<MrPuppetDataMapper>();
        }

        private void Update()
        {
            // TODO: should JawPercent be smoothed, and/or position and rotation?

            _jawPercentSmoothed = Mathf.SmoothDamp(_jawPercentSmoothed, DataMapper.JawPercent, ref _jawPercentVelocity, SmoothTime);
            if (ApplySensors == true)
            {
                JawJoint.localRotation = Quaternion.LerpUnclamped(
                    AnimData.CloseRotation,
                    AnimData.OpenRotation,
                    _jawPercentSmoothed);
            }
        }

        private void OnJawJointAssigned()
        {
            if (JawJoint == null) return;
            AnimData = new JawAnimData()
            {
                OpenRotation = JawJoint.localRotation,
                CloseRotation = JawJoint.localRotation
            };
        }

#if UNITY_EDITOR
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
#endif
    }

#if UNITY_EDITOR
    // Editor
    [CustomEditor(typeof(JawTransformMapper))]
    public class JawTransformMapperEditor : OdinEditor
    {
        private class JawAnimDataEdit
        {
            public Quaternion OriginalRotation;
            public bool EditOpenPose;
            public bool EditClosePose;
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

            _editJawMode = _jawAnimDataEdit.EditOpenPose || _jawAnimDataEdit.EditClosePose;

            // Edit buttons
            EditorGUILayout.BeginHorizontal();
            EditTransformButton("Edit Open Pose", ref _jawTransformMapper.JawJoint, ref _jawTransformMapper.AnimData.OpenRotation, ref _jawAnimDataEdit.EditOpenPose, ref _jawAnimDataEdit.OriginalRotation);
            EditTransformButton("Edit Close Pose", ref _jawTransformMapper.JawJoint, ref _jawTransformMapper.AnimData.CloseRotation, ref _jawAnimDataEdit.EditClosePose, ref _jawAnimDataEdit.OriginalRotation);
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
            if (_jawAnimDataEdit != null && (_jawAnimDataEdit.EditOpenPose || _jawAnimDataEdit.EditClosePose))
            {
                _jawTransformMapper.JawJoint.rotation = Handles.RotationHandle(_jawTransformMapper.JawJoint.rotation, _jawTransformMapper.JawJoint.position);
            }
        }

        private void EditTransformButton(string button, ref Transform transf, ref Quaternion rot, ref bool edit, ref Quaternion originalRotation)
        {
            GUI.enabled = (edit || !_editJawMode) && !_previewJawMode && !Application.isPlaying;

            var defColor = GUI.color;
            GUI.color = edit ? Color.green : Color.white;

            if (GUILayout.Button(button))
            {
                edit = !edit;
                if (edit)
                {
                    _jawAnimDataEdit.OriginalRotation = _jawTransformMapper.JawJoint.localRotation;

                    originalRotation = transf.localRotation;
                    transf.localRotation = rot;
                    Tools.hidden = true; // Hides the default gizmos so they don't get in the way
                }
                else
                {
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
                _jawTransformMapper.JawJoint.localRotation = _jawAnimDataEdit.OriginalRotation;
            }
        }

        private void JawStep(float step)
        {
            _jawTransformMapper.JawJoint.localRotation = Quaternion.Lerp(_jawTransformMapper.AnimData.OpenRotation, _jawTransformMapper.AnimData.CloseRotation, step);
        }
    }
#endif
}
