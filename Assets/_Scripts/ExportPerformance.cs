using System.Collections.Generic;
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

        public enum Rating { Trash, Blooper, Keeper };

        [TableList(DefaultMinColumnWidth = 160)]
        public List<ExportTake> Exports = new List<ExportTake>();

        private GameObject RecorderTarget;
        private string Filename;
        private RecorderWindow Recorder;
        private bool StartedRecording;
        private bool PromptShowing;

        [Serializable]
        public class ExportTake
        {
            [PreviewField]
            [TableColumnWidth(60, Resizable = false)]
            public GameObject _Prefab;

            [TableColumnWidth(160)]
            [VerticalGroup("Animation and Rating")]
            [HideLabel]
            public AnimationClip _Animation;

            [TableColumnWidth(160)]
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
                Exports.Add(new ExportPerformance.ExportTake((AnimationClip)AssetDatabase.LoadAssetAtPath(filename, typeof(AnimationClip)), RecorderTarget, Rating.Keeper));
                RecorderPrompt.ShowUtilityWindow(this);
            }

            Repaint();
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

            string DataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/HyperMesh/Performances/";
            if (!Directory.Exists(DataPath))
                DataPath = "Performances/";

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
                    ModelExporter.ExportObject(DataPath + export._Animation.name + ".fbx", instance);

                    // cleanup
                    DestroyImmediate(instance);
                    AssetDatabase.DeleteAsset("Assets/Recordings/" + export._Animation.name + ".controller");
                    FileUtil.MoveFileOrDirectory("Assets/Recordings/" + export._Animation.name + ".anim", DataPath + export._Animation.name + ".anim");
                }
                else
                {
                    FileUtil.MoveFileOrDirectory("Assets/Recordings/" + export._Animation.name + ".anim", DataPath + export._Animation.name + ".anim");
                }

                // write file
                var sr = File.CreateText(DataPath + export._Animation.name + ".csv");
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
            [DisplayAsString, ShowInInspector, BoxGroup("Prompt", false), HideLabel]
            public static string PromptBox;
            [DisplayAsString, ShowInInspector, BoxGroup, HideLabel]
            public static string InstructionsLineOne;
            [DisplayAsString, ShowInInspector, BoxGroup, HideLabel]
            public static string InstructionsLineTwo;

            private static string filename;
            private static ExportPerformance ExportPerformanceInstance;
            private static RecorderPrompt window;

            public static void ShowUtilityWindow(ExportPerformance instance)
            {
                ExportPerformanceInstance = instance;
                window = (RecorderPrompt)EditorWindow.GetWindow(typeof(RecorderPrompt), true, "Rate Take");
                window.ShowUtility();
                window.position = new Rect((Screen.currentResolution.width / 2) - (320 / 2), (Screen.currentResolution.height / 2) - (100 / 2), 320, 100);

                InstructionsLineOne = "Select “Keeper (↵)” or “Blooper (B)” to create FBX of take.";
                InstructionsLineTwo = "Select “Trash (T)” to skip FBX.";
                filename = "Assets/Recordings/" + ExportPerformanceInstance.Filename + ".anim";
                PromptBox = "TAKE: " + ExportPerformanceInstance.Filename;

                ExportPerformanceInstance.PromptShowing = true;
            }

            [HorizontalGroup]
            [Button("Trash (T)", ButtonSizes.Medium)]
            public void Trash()
            {
                ExportPerformanceInstance.Exports[ExportPerformanceInstance.Exports.Count - 1]._Rating = ExportPerformance.Rating.Trash;
                Close();
            }

            [HorizontalGroup]
            [Button("Blooper (B)", ButtonSizes.Medium)]
            public void Blooper()
            {
                ExportPerformanceInstance.Exports[ExportPerformanceInstance.Exports.Count - 1]._Rating = ExportPerformance.Rating.Blooper;
                Close();
            }

            [HorizontalGroup]
            [GUIColor(0.5f, 0.8f, 0.5f)]
            [Button("Keeper (↵)", ButtonSizes.Medium)]
            public void Keeper()
            {
                ExportPerformanceInstance.Exports[ExportPerformanceInstance.Exports.Count - 1]._Rating = ExportPerformance.Rating.Keeper;
                Close();
            }

            private void OnDestroy()
            {
                ExportPerformanceInstance.PromptShowing = false;
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
