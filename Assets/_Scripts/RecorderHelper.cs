using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Recorder;
using UnityEngine.SceneManagement;
using System.Collections;

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
    //[CustomEditor(typeof(RecorderHelper))]
    public class RecorderHelper : OdinEditorWindow
    {
        [MenuItem("Tools/Recorder Helper")]
        private static void OpenWindow()
        {
            GetWindow<RecorderHelper>().Show();
        }

        private RecorderWindow Recorder;
        private static string ButtonMessage;
        private bool ShowInfo;
        private static int CountdownValue;

        [DisplayAsString, ShowInInspector, BoxGroup, HideLabel, ShowIf("ShowInfo")]
        public string StatusBox;

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

        [Button("$ButtonMessage", 300)]
        [GUIColor(1, 0, 0)]
        [DisableInEditorMode]
        private void Action()
        {
            if (Recorder != EditorWindow.GetWindow<RecorderWindow>())
                Recorder = EditorWindow.GetWindow<RecorderWindow>();

            if (!Recorder.IsRecording())
            {
                if (CountdownValue < 0)
                { 
                    Camera.main.gameObject.AddComponent<CountDown>();
                    Camera.main.gameObject.GetComponent<MonoBehaviour>().StartCoroutine(StartCountdown());
                }
            }
            else
                ControlRecording();

            Debug.Log(Recorder);

        }

        private void ControlRecording()
        {
            if (Recorder != EditorWindow.GetWindow<RecorderWindow>())
                Recorder = EditorWindow.GetWindow<RecorderWindow>();

            if (!Recorder.IsRecording())
            {
                Recorder.StartRecording();
            }
            else
            {
                Recorder.StopRecording();
                AssetDatabase.SaveAssets();
            }
        }

        private void Update()
        {
            if (EditorApplication.isPlaying == true)
            {
                ShowInfo = true;

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Action();
                }

                if (Recorder)
                {
                    Debug.Log("int update33333");

                    if (Recorder.IsRecording())
                    {
                        GetFilename();
                        ButtonMessage = "STOP";
                    }
                    else
                    {
                        if (CountdownValue < 0)
                        {
                            StatusBox = "READY TO REC";
                            ButtonMessage = "START";
                        }
                    }
                }
            }
            else
            {
                CountdownValue = -1;
                ShowInfo = false;
            }

            Repaint();
        }

        public class CountDown : MonoBehaviour
        {

        }

        public IEnumerator StartCountdown()
        {

            CountdownValue = 3;
            while (CountdownValue > -1)
            {
                if (Recorder)
                    if (Recorder.IsRecording())
                    {
                        //EditorUtility.DisplayDialog("Recording while attempting a record", "Stopped your recording", "OK");
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
                CountdownValue = -1;
            }

            private static void playModes(PlayModeStateChange state)
            {
                if (state == UnityEditor.PlayModeStateChange.EnteredPlayMode)
                {
                    //ShowInfo = false;
                    ButtonMessage = "START";
                }
                else
                {
                    //ShowInfo = true;
                    ButtonMessage = "DISABLED";
                }
            }
        }

    }
#else
public class PerformanceAnimationReplacementManager : MonoBehaviour {

}
#endif
}
