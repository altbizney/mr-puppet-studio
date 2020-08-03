using UnityEngine;
using UnityEditor.Recorder;
using UnityEngine.SceneManagement;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
#endif

// TODO:
//       Key commands that are not tied to game view

namespace MrPuppet
{
#if UNITY_EDITOR
    public class RecorderHelper : OdinEditorWindow
    {
        [MenuItem("Tools/Recorder Helper")]
        private static void OpenWindow()
        {
            GetWindow<RecorderHelper>().Show();
            GetWindow<ExportPerformance>().Show();
        }

        void OnEnable()
        {
            Instance = this;
        }

        public enum AudioModes { AudRef, OneShot};


        private RecorderWindow Recorder;
        private static string ButtonMessage;
        //private bool ShowInfo;
        private static int CountdownValue;
        private static Color ColorState;
        private AudioClip _AudioClip;
        private ExportPerformance _ExportPerformance;

        [DisplayAsString, ShowInInspector, BoxGroup, HideLabel]
        public static string StatusBox;

        public static RecorderHelper Instance { get; private set; }
        public static bool IsOpen { get { return Instance != null; } }

        [EnumToggleButtons]
        [HideLabel]
        [HorizontalGroup]
        [OnValueChanged("ChangeModes")]
        public AudioModes Mode;

        [Range(0, 1)]
        [HorizontalGroup]
        [HideLabel]
        public float AudioVolume;

        private OneShotsWindow OneShots;
        private AudioReference AudioRef;

        private void ChangeModes()
        {
            
            if (EditorApplication.isPlaying){
                
                if(!AudioRef)
                    AudioRef = FindObjectOfType<AudioReference>();

                if (Mode == AudioModes.OneShot){
                    AudioRef.enabled = false;
                }
                if (Mode == AudioModes.AudRef){
                    AudioRef.enabled = true;
                }
            }
            
        }

        private void GetFilename()
        {
            RecorderControllerSettings m_ControllerSettings = RecorderControllerSettings.LoadOrCreate(Application.dataPath + "/../Library/Recorder/recorder.pref");
            RecorderController m_RecorderController = new RecorderController(m_ControllerSettings);
            foreach (var recorder in m_RecorderController.Settings.RecorderSettings)
            {
                if (!recorder.Enabled) continue;

                StatusBox = recorder.OutputFile;

                StatusBox = StatusBox.Replace("<Take>", recorder.Take.ToString("000"));
                StatusBox = StatusBox.Replace("<Scene>", SceneManager.GetActiveScene().name);

                StatusBox = "Recording: " + StatusBox.Substring(StatusBox.LastIndexOf('/') + 1);

                // just need the first
                return;
            }
        }

        [Button("$ButtonMessage", 150)]
        [GUIColor("ColorState")]
        [DisableInEditorMode]
        private void Action()
        {
            try
            { Recorder = EditorWindow.GetWindow<RecorderWindow>(); }
            catch { throw; }

            if (_ExportPerformance != EditorWindow.GetWindow<ExportPerformance>())
                _ExportPerformance = EditorWindow.GetWindow<ExportPerformance>();

            if (!Recorder.IsRecording())
            {
                if (CountdownValue < 1)
                {
                    _AudioClip = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/_SFX/beep-01.mp3", typeof(AudioClip));

                    Camera.main.gameObject .AddComponent<CountDown>();
                    Camera.main.gameObject.GetComponent<MonoBehaviour>().StartCoroutine(StartCountdown());
                }
            }
            else
                ControlRecording();

            EditorApplication.ExecuteMenuItem("Window/General/Game");
        }

        private void ControlRecording()
        {
            ColorState = Color.red;

            try
            { Recorder = EditorWindow.GetWindow<RecorderWindow>(); }
            catch { throw; }

            if (OneShots != EditorWindow.GetWindow<OneShotsWindow>())
                OneShots = EditorWindow.GetWindow<OneShotsWindow>();

            if(Recorder)
            {
                if (!Recorder.IsRecording())
                {
                    Recorder.StartRecording();
                    EditorApplication.ExecuteMenuItem("Window/General/Game");
                    ButtonMessage = "STOP";
                    Repaint();

                    if (Mode == AudioModes.OneShot)
                        OneShots.Record();
                }
                else
                {
                    Recorder.StopRecording();
                    AssetDatabase.SaveAssets();
                    StatusBox = "READY TO REC";
                    ButtonMessage = "START";
                    Repaint();

                    if (Mode == AudioModes.OneShot)
                        OneShots.Stop();
                }
            }
        }

        private void Update()
        {
            if (EditorApplication.isPlaying == true)
            {
                //ShowInfo = true;

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Action();
                }

                if (Recorder)
                {
                    if (Recorder.IsRecording())
                    {
                        GetFilename();
                        ButtonMessage = "STOP";
                    }
                    else
                    {
                        if (CountdownValue < 1)
                        {
                            StatusBox = "READY TO REC";
                            ButtonMessage = "START";
                            ColorState = Color.green;
                        }
                    }
                }
                //ColorState = Color.red;
            }
            else
            {
                CountdownValue = 0;
                //ShowInfo = false;
                ColorState = Color.gray;
            }
        }

        public class CountDown : MonoBehaviour
        {

        }

        public IEnumerator StartCountdown()
        {
            ColorState = Color.yellow;
            CountdownValue = 3;
            while (CountdownValue > 0)
            {
                Repaint();
                AudioSource.PlayClipAtPoint(_AudioClip, Camera.main.gameObject.transform.position, AudioVolume);

                if (Recorder)
                    if (Recorder.IsRecording())
                    {
                        Recorder.StopRecording();
                        CountdownValue = -1;
                        yield break;
                    }

                StatusBox = "Recorder Starting in..." + CountdownValue.ToString();
                ButtonMessage = CountdownValue.ToString();
                if (CountdownValue == 0)
                    StatusBox = "STARTING RECORDING!";
                yield return new WaitForSeconds(1.0f);
                CountdownValue--;
            }

            ControlRecording();
        }

        [InitializeOnLoadAttribute]
        static class PlayModeStateChanged
        {
            static PlayModeStateChanged()
            {
                EditorApplication.playModeStateChanged += playModes;
                ButtonMessage = "DISABLED";
                StatusBox = "Enter play mode to record";

                CountdownValue = 0;
                ColorState = Color.gray;
            }

            private static void playModes(PlayModeStateChange state)
            {
                if (state == UnityEditor.PlayModeStateChange.EnteredPlayMode)
                {
                    //ShowInfo = false;
                    ButtonMessage = "START";
                    ColorState = Color.green;
                    if (Instance)
                        Instance.ChangeModes();
                }
                else
                {
                    //ShowInfo = true;
                    //Instance.Repaint();
                    StatusBox = "Enter play mode to record";
                    ButtonMessage = "DISABLED";
                    CountdownValue = 0;
                }
            }
        }
    }
#else
    public class PerformanceAnimationReplacementManager : MonoBehaviour
    {

    }
#endif
}
