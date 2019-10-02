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
            public Quaternion OpenMaxRotation = Quaternion.identity;
            public Quaternion CloseRotation = Quaternion.identity;
            public Quaternion CloseMaxRotation = Quaternion.identity;
        }

        [Required]
        public MrPuppetDataMapper DataMapper;

        public bool EnableDebugGraph = false;

        [Required]
        [OnValueChanged(nameof(OnJawJointAssigned))]
        public Transform JawJoint;

        [HideInInspector]
        public JawAnimData AnimData;

        [HorizontalGroup("JawSpring")]
        public bool EnableJawSpring = true;

        [ShowIf("EnableJawSpring")]
        public float JawStiffness = 100.0f;

        [ShowIf("EnableJawSpring")]
        public float JawDamping = 20.0f;

        public float JawSmoothTime = 0.05f;

        private float JawVelocity;
        private float JawCurrent;

        private void OnValidate()
        {
            if (DataMapper == null) DataMapper = FindObjectOfType<MrPuppetDataMapper>();
        }

        private void Start()
        {
            // preload spring to avoid initial wobble
            JawCurrent = DataMapper.JawPercent;
        }

        private void Update()
        {
            if (EnableJawSpring)
            {
                // JawCurrent = Springz.Float(JawCurrent, DataMapper.JawPercent, ref JawVelocity, JawStiffness, JawDamping);
                JawCurrent = Mathf.SmoothDamp(JawCurrent, DataMapper.JawPercent, ref JawVelocity, JawSmoothTime);
            }
            else
            {
                JawCurrent = DataMapper.JawPercent;
            }

            JawJoint.localRotation = Quaternion.SlerpUnclamped(AnimData.CloseRotation, AnimData.OpenRotation, JawCurrent);

            if (EnableDebugGraph)
            {
                DebugGraph.MultiLog("Jaw", Color.red, JawCurrent, "Current");
                DebugGraph.MultiLog("Jaw", Color.blue, DataMapper.JawPercent, "Target");
            }
        }

        private void OnJawJointAssigned()
        {
            if (JawJoint == null) return;
            AnimData = new JawAnimData()
            {
                OpenRotation = JawJoint.localRotation,
                OpenMaxRotation = JawJoint.localRotation,
                CloseRotation = JawJoint.localRotation,
                CloseMaxRotation = JawJoint.localRotation
            };
        }

#if UNITY_EDITOR
        // The section below is used to store the changes made at runtime
        static JawTransformMapper()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private const string JawSmoothTimeKey = "JawSmoothTime";
        private const string JawDampingKey = "JawDamping";
        private const string JawStiffnessKey = "JawStiffness";

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            var jawTransformMapper = FindObjectOfType<JawTransformMapper>();
            if (jawTransformMapper == null) return;

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                Undo.RecordObject(jawTransformMapper, "Undo JawTransformMapper");
                jawTransformMapper.JawSmoothTime = PlayerPrefs.GetFloat(JawSmoothTimeKey);
                jawTransformMapper.JawDamping = PlayerPrefs.GetFloat(JawDampingKey);
                jawTransformMapper.JawStiffness = PlayerPrefs.GetFloat(JawStiffnessKey);
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                PlayerPrefs.SetFloat(JawSmoothTimeKey, jawTransformMapper.JawSmoothTime);
                PlayerPrefs.SetFloat(JawDampingKey, jawTransformMapper.JawDamping);
                PlayerPrefs.SetFloat(JawStiffnessKey, jawTransformMapper.JawStiffness);
            }
        }
#endif
    }

    // Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(JawTransformMapper))]
    public class JawTransformMapperEditor : OdinEditor
    {
        private class JawAnimDataEdit
        {
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
            // EditTransformButton("Edit Open Max", ref _jawTransformMapper.JawJoint, ref _jawTransformMapper.AnimData.OpenMaxRotation, ref _jawAnimDataEdit.EditOpenMax, ref _jawAnimDataEdit.OriginalRotation);
            EditTransformButton("Edit Open Pose", ref _jawTransformMapper.JawJoint, ref _jawTransformMapper.AnimData.OpenRotation, ref _jawAnimDataEdit.EditOpenPose, ref _jawAnimDataEdit.OriginalRotation);
            EditTransformButton("Edit Close Pose", ref _jawTransformMapper.JawJoint, ref _jawTransformMapper.AnimData.CloseRotation, ref _jawAnimDataEdit.EditClosePose, ref _jawAnimDataEdit.OriginalRotation);
            // EditTransformButton("Edit Close Max", ref _jawTransformMapper.JawJoint, ref _jawTransformMapper.AnimData.CloseMaxRotation, ref _jawAnimDataEdit.EditCloseMax, ref _jawAnimDataEdit.OriginalRotation);
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
                rot = Handles.Disc(rot, pos, _jawTransformMapper.JawJoint.forward, HandleUtility.GetHandleSize(pos) * 2f, false, 0.5f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_jawTransformMapper.JawJoint, "Adjust jaw");
                    _jawTransformMapper.JawJoint.rotation = rot;
                }
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
            _jawTransformMapper.JawJoint.localRotation = Quaternion.Slerp(_jawTransformMapper.AnimData.OpenRotation, _jawTransformMapper.AnimData.CloseRotation, step);
        }
    }
#endif
}