﻿using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine.SceneManagement;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Formats.Fbx.Exporter;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
#endif

namespace MrPuppet
{
#if UNITY_EDITOR
    public class ExportPerformance : OdinEditorWindow
    {
        [MenuItem("Tools/Export Performances")]
        private static void OpenWindow()
        {
            GetWindow<ExportPerformance>().Show();
        }

        public enum Rating { Trash, Keeper, Blooper };

        [TableList]
        public List<ExportTake> Exports = new List<ExportTake>();

        private GameObject RecorderTarget;
        private string Filename;
        private RecorderWindow Recorder;
        private bool StartedRecording;

        [Serializable]
        public class ExportTake
        {
            [PreviewField]
            [TableColumnWidth(60, Resizable = false)]
            public GameObject _Prefab;

            [TableColumnWidth(140)]
            [VerticalGroup("Animation and Rating")]
            [HideLabel]
            public AnimationClip _Animation;

            [TableColumnWidth(140)]
            [EnumToggleButtons]
            [VerticalGroup("Animation and Rating")]
            [HideLabel]
            public Rating _Rating;

            public ExportTake(AnimationClip ConstructorAnimation, GameObject ConstructorPrefab, Rating ConstructorRating)
            {
                _Animation = ConstructorAnimation;
                _Prefab = ConstructorPrefab;
                _Rating = ConstructorRating;
            }
        }

        void Update()
        {
            if (Recorder == null)
                Recorder = EditorWindow.GetWindow<RecorderWindow>();

            if (Recorder.IsRecording() && StartedRecording == false)
            {
                StartedRecording = true;
                GetFilename();
            }
            if (!Recorder.IsRecording() && StartedRecording == true)
            {
                StartedRecording = false;
                var filename = "Assets/Recordings/" + Filename + ".anim";
                Exports.Add(new ExportPerformance.ExportTake((AnimationClip)AssetDatabase.LoadAssetAtPath(filename, typeof(AnimationClip)), RecorderTarget, Rating.Trash));
                RecorderPrompt.ShowUtilityWindow(this);
            }
        }

        private void OnDestroy()
        {
            var win = Instantiate<ExportPerformance>(this);
            if (RecorderHelper.IsOpen)
            {
                win.Show();
            }
        }

        private void GetFilename()
        {
            RecorderControllerSettings m_ControllerSettings = RecorderControllerSettings.LoadOrCreate(Application.dataPath + "/../Library/Recorder/recorder.pref");
            RecorderController m_RecorderController = new RecorderController(m_ControllerSettings);

            foreach (var recorder in m_RecorderController.Settings.RecorderSettings)
            {
                if (!recorder.Enabled) continue;

                Filename = recorder.OutputFile;

                foreach (var input in recorder.InputsSettings) { RecorderTarget = ((AnimationInputSettings)input).gameObject; }

                Filename = Filename.Replace("<Take>", recorder.Take.ToString("000"));
                Filename = Filename.Replace("<Scene>", SceneManager.GetActiveScene().name);

                Filename = Filename.Substring(Filename.LastIndexOf('/') + 1);

                // just need the first
                return;
            }
        }

        [Button(ButtonSizes.Large)]
        private void Export()
        {
            int success = 0;
            AssetDatabase.SaveAssets();

            foreach (var export in Exports)
            {
                if (export._Rating != Rating.Trash)
                {
                    // create controller
                    var controller = AnimatorController.CreateAnimatorControllerAtPathWithClip("Assets/Recordings/" + export._Animation.name + ".controller", export._Animation);

                    // create instance, rename
                    var instance = GameObject.Instantiate(export._Prefab, Vector3.zero, Quaternion.identity);
                    instance.name = export._Animation.name;

                    // add animator to instance
                    var animator = instance.AddComponent<Animator>();
                    animator.runtimeAnimatorController = controller;

                    // export
                    ModelExporter.ExportObject("Performances/" + export._Animation.name + ".fbx", instance);

                    // cleanup
                    DestroyImmediate(instance);
                    AssetDatabase.DeleteAsset("Assets/Recordings/" + export._Animation.name + ".controller");
                    FileUtil.MoveFileOrDirectory("Assets/Recordings/" + export._Animation.name + ".anim", "Performances/" + export._Animation.name + ".anim");
                }
                else
                {
                    FileUtil.MoveFileOrDirectory("Assets/Recordings/" + export._Animation.name + ".anim", "Performances/" + export._Animation.name + ".anim");
                }

                // write file
                var sr = File.CreateText("Performances/" + export._Animation.name + ".csv");
                sr.WriteLine(export._Animation.name + "," + export._Prefab.name + "," + export._Rating);
                sr.Close();

                success++;
            }

            if (success == Exports.Count)
            {
                Exports = new List<ExportTake>();
            }
        }

        public class RecorderPrompt : OdinEditorWindow
        {
            [DisplayAsString, ShowInInspector, BoxGroup, HideLabel]
            public static string PromptBox;

            private static string filename;
            private static ExportPerformance ExportPerformanceInstance;

            public static void ShowUtilityWindow(ExportPerformance instance)
            {
                ExportPerformanceInstance = instance;
                RecorderPrompt window = ScriptableObject.CreateInstance(typeof(RecorderPrompt)) as RecorderPrompt;
                window.ShowUtility();
                window.position = new Rect((Screen.currentResolution.width / 2) - (170 / 2), (Screen.currentResolution.height / 2) - (90 / 2), 170, 90);
                filename = "Assets/Recordings/" + ExportPerformanceInstance.Filename + ".anim";
                PromptBox = "TAKE: " + ExportPerformanceInstance.Filename;
            }

            [Button("Keeper")]
            public void Keeper()
            {
                ExportPerformanceInstance.Exports[ExportPerformanceInstance.Exports.Count - 1]._Rating = ExportPerformance.Rating.Keeper;
                Close();
            }

            [Button("Blooper")]
            public void Blooper()
            {
                ExportPerformanceInstance.Exports[ExportPerformanceInstance.Exports.Count - 1]._Rating = ExportPerformance.Rating.Blooper;
                Close();
            }

            [Button("Trash")]
            public void Trash()
            {
                ExportPerformanceInstance.Exports[ExportPerformanceInstance.Exports.Count - 1]._Rating = ExportPerformance.Rating.Trash;
                Close();
            }

            private void OnGUI()
            {
                base.OnGUI();

                Event current = Event.current;

                switch (current.keyCode)
                {
                    case KeyCode.Return:
                        Keeper();
                        break;
                    case KeyCode.T:
                        Trash();
                        break;
                    case KeyCode.B:
                        Blooper();
                        break;
                }
            }

            void OnInspectorUpdate()
            {
                Repaint();
            }
        }
    }
#else
public class ExportPerformance : MonoBehaviour {

}
#endif
}
