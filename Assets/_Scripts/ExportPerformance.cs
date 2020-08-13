using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine.SceneManagement;
using System.Linq;
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
        [PropertyOrder(1)]
        [OnValueChanged("SetList")]
        public List<ExportTake> Exports = new List<ExportTake>();

        private GameObject PrefabOverwrite;
        private string AnimationOverwrite;

        private GameObject RecorderTarget;
        private string Filename;
        private RecorderWindow Recorder;
        private bool StartedRecording;

        private static bool OneShotsRecording;
        private static GameObject OneShotTarget;
        private static string OneShotName;

        [SerializeField]
        //[HideInInspector]
        private List<string> AnimationNames = new List<string>();


        public static ExportPerformance Instance { get; private set; }

        /*
        public static bool IsOpen {
         get { return Instance != null; }
        }
        */

        void OnEnable()
        {
            Instance = this;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
        }

        void OnDisable() {
            Instance = null;
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
        }

        private void Awake() {
            for(int i=0; i<Exports.Count; i++)
            {
                Exports[i]._Animation = (AnimationClip)AssetDatabase.LoadAssetAtPath("Assets/Recordings/" + AnimationNames[i] + ".anim", typeof(AnimationClip));
            }
        }

        public void OnAfterAssemblyReload()
        {
            Debug.Log("After Assembly Reload");

            for(int i=0; i<Exports.Count; i++)
            {
                Exports[i]._Animation = (AnimationClip)AssetDatabase.LoadAssetAtPath("Assets/Recordings/" + AnimationNames[i] + ".anim", typeof(AnimationClip));
            }
        }

        public void SetList()
        {
            AnimationNames = new List<string>();
            foreach(ExportTake export in Exports)
                AnimationNames.Add(export._Animation.name);
        }

        [Serializable]
        public class ExportTake
        {
            [PreviewField]
            [TableColumnWidth(65, Resizable = false)]
            [VerticalGroup("Preview")]
            [HideLabel]
            public GameObject _Prefab;

            [TableColumnWidth(160)]
            [VerticalGroup("Animation and Rating")]
            [HideLabel]
            [OnValueChanged("SetList")]
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

            [Button(ButtonSizes.Small)]
            [VerticalGroup("Preview")]
            [GUIColor(0.2f, 0.9f, 0.2f)]
            private void OneShots()
            {
                GetWindow<OneShotsWindow>().Show();
                OneShotsWindow Instance = EditorWindow.GetWindow<OneShotsWindow>();
                Instance.Actor = _Prefab;
                Instance.Performance = _Animation;
                Instance.ParseAnimationClip();
            }

            public void SetList()
            {
                Instance.AnimationNames = new List<string>();
                foreach(ExportTake export in Instance.Exports)
                   Instance.AnimationNames.Add(export._Animation.name);
            }

        }

        public void UpdateExport(GameObject _PrefabOverwrite, string _AnimationOverwrite)
        {
            //Allow for wider case scenarios and uses
            PrefabOverwrite = _PrefabOverwrite;
            AnimationOverwrite = _AnimationOverwrite;
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
                var filename = "Assets/Recordings/" + Filename + ".anim";

                if (PrefabOverwrite)
                {
                    RecorderTarget = PrefabOverwrite;
                    PrefabOverwrite = null;
                }

                StartedRecording = false;
                ExportTake currentTake = new ExportPerformance.ExportTake((AnimationClip)AssetDatabase.LoadAssetAtPath(filename, typeof(AnimationClip)), RecorderTarget, Rating.Keeper);
                Exports.Add(currentTake);
                AnimationNames.Add(currentTake._Animation.name);

                if (AnimationOverwrite != null)
                {
                    AssetDatabase.RenameAsset(filename, AnimationOverwrite);
                    AnimationOverwrite = null;
                }

                RecorderPrompt.ShowUtilityWindow(this);
            }
        }

        /*
        private void OnDestroy()
        {
            //var win = Instantiate<ExportPerformance>(this);
            //if (RecorderHelper.IsOpen)
            //{
              //  win.Show();
            //}
        }
        */

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
        [PropertyOrder(-1)]
        private void Export()
        {
            AssetDatabase.SaveAssets();

            string DataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/HyperMesh/Performances/";
            if (!Directory.Exists(DataPath))
                DataPath = "Performances/";


            foreach (var export in Exports.ToList())
            {
                //if rating is null there would be an issue. but that shouldnt be possible
                bool errorFound = false;

                if (export._Rating != Rating.Trash)
                {
                    GameObject instance = null;
                    try
                    {
                        // create controller
                        var controller = AnimatorController.CreateAnimatorControllerAtPathWithClip("Assets/Recordings/" + export._Animation.name + ".controller", export._Animation);

                        // create instance, rename
                        instance = GameObject.Instantiate(export._Prefab, Vector3.zero, Quaternion.identity);
                        instance.name = export._Animation.name;

                        // add animator to instance
                        if (instance.GetComponent<Animator>() != null)
                            DestroyImmediate(instance.GetComponent<Animator>());

                        var animator = instance.AddComponent<Animator>();
                        animator.runtimeAnimatorController = controller;

                        // export
                        ModelExporter.ExportObject(DataPath + export._Animation.name + ".fbx", instance);
                    }
                    catch(Exception ex)
                    {
                        errorFound = true;
                        Debug.LogError("<color=red>Export Error: </color> " + ex);
                    }

                    // cleanup
                    if(export._Animation)
                        AssetDatabase.DeleteAsset("Assets/Recordings/" + export._Animation.name + ".controller");
                    if(instance != null)
                        DestroyImmediate(instance);
                }

                if (errorFound == false)
                {
                    // write file
                    var sr = File.CreateText(DataPath + export._Animation.name + ".csv");
                    sr.WriteLine(export._Animation.name + "," + export._Prefab.name + "," + export._Rating);
                    sr.Close();

                    // move animation regardless of rating
                    // what if already exists?
                    FileUtil.MoveFileOrDirectory("Assets/Recordings/" + export._Animation.name + ".anim", DataPath + export._Animation.name + ".anim");

                    // update datastructures with sucessful export
                    Exports.Remove(export);
                    AnimationNames.Remove(export._Animation.name);
                }
                else
                    break;
            }
        }

        /*
        [Button(ButtonSizes.Large)]
        private void Remove()
        {
            Exports.RemoveAt(0);
        }
        */

        //when remove clip thorugh a draggin in a new clip, or when selecting none
        //remove clip when you just exist that exportTake out

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
                window.position = new Rect((Screen.currentResolution.width / 2) - (320 / 2), (Screen.currentResolution.height / 2) - (100 / 2), 340, 100);

                InstructionsLineTwo = "Select “Blooper (W)” or “Keeper (↵, E)” to create FBX of take.";
                InstructionsLineOne = "Select “Trash (Q)” to skip FBX.";
                filename = "Assets/Recordings/" + ExportPerformanceInstance.Filename + ".anim";
                PromptBox = "TAKE: " + ExportPerformanceInstance.Filename;

                //ExportPerformanceInstance.PromptShowing = true;
            }

            [HorizontalGroup]
            [Button("Trash (Q)", ButtonSizes.Medium)]
            public void Trash()
            {
                ExportPerformanceInstance.Exports[ExportPerformanceInstance.Exports.Count - 1]._Rating = ExportPerformance.Rating.Trash;
                ExportPerformanceInstance.Repaint();
                Close();
            }

            [HorizontalGroup]
            [Button("Blooper (W)", ButtonSizes.Medium)]
            public void Blooper()
            {
                ExportPerformanceInstance.Exports[ExportPerformanceInstance.Exports.Count - 1]._Rating = ExportPerformance.Rating.Blooper;
                ExportPerformanceInstance.Repaint();
                Close();
            }

            [HorizontalGroup]
            [GUIColor(0.5f, 0.8f, 0.5f)]
            [Button("Keeper (↵, E)", ButtonSizes.Medium)]
            public void Keeper()
            {
                ExportPerformanceInstance.Exports[ExportPerformanceInstance.Exports.Count - 1]._Rating = ExportPerformance.Rating.Keeper;
                ExportPerformanceInstance.Repaint();
                Close();
            }

            private void OnDestroy()
            {
                //ExportPerformanceInstance.PromptShowing = false;
                ExportPerformanceInstance.Repaint();
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
                    case KeyCode.Q:
                        Trash();
                        break;
                    case KeyCode.W:
                        Blooper();
                        break;
                    case KeyCode.E:
                        Keeper();
                        break;
                }
            }
        }
    }
#else
    public class ExportPerformance : MonoBehaviour
    {

    }
#endif
}
