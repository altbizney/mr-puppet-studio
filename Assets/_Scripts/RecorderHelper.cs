using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Recorder;
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
        private static bool ShowInfo;

        /*[SerializeField]
        [DisplayAsString]
        [HideLabel]
        [BoxGroup]
        [GUIColor(1, 0, 0)]
        //[LabelWidth(300)]
        private string InfoBoxMsg;*/

        //private static int ButtonHeight;

        [DisplayAsString, ShowInInspector, BoxGroup, ShowIf("ShowInfo")]
        public string Filename;

        private void GetFilename()
        {
            RecorderControllerSettings m_ControllerSettings = RecorderControllerSettings.LoadOrCreate(Application.dataPath + "/../Library/Recorder/recorder.pref");
            RecorderController m_RecorderController = new RecorderController(m_ControllerSettings);

            foreach (var recorder in m_RecorderController.Settings.RecorderSettings)
            {
                if (!recorder.Enabled) continue;

                Filename = recorder.OutputFile;

                Filename = Filename.Replace("<Take>", recorder.Take.ToString("000"));
                Filename = Filename.Replace("<Scene>", SceneManager.GetActiveScene().name);

                Filename = Filename.Substring(Filename.LastIndexOf('/') + 1);

                // just need the first
                return;
            }
        }

        //Set Button Size to Window Size? Dynamic Size? 
        [Button("$ButtonMessage", 300)]
        [GUIColor(1, 0, 0)]
        [DisableInEditorMode]
        private void Action()
        {
            if (Recorder != EditorWindow.GetWindow<RecorderWindow>())
                Recorder = EditorWindow.GetWindow<RecorderWindow>();

            if (!Recorder.IsRecording())
            {
                Recorder.StartRecording();
                ButtonMessage = "RECORDING";
            }
            else
            {
                Recorder.StopRecording();
                //EditorSceneManager.SaveScene( SceneManager.GetActiveScene() );
                //EditorApplication.SaveScene();
                AssetDatabase.SaveAssets();
                ButtonMessage = "READY TO REC";
            }

            var win = GetWindow<RecorderHelper>();
        }

        // Possible global button press solution.
        /*
        void OnSceneGUI()
        {
            //RecorderHelper script = (RecorderHelper)target;
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.KeyDown:
                    {
                        if (Event.current.keyCode == (KeyCode.Space))
                        {
                            //script.Painting = true;
                        }
                        break;
                    }
            }
        }
        */

        private void Update()
        {
            if (EditorApplication.isPlaying == true)
            {
                GetFilename();

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Action();
                }


                if (Recorder)
                {
                    if (Recorder.IsRecording())
                        ShowInfo = true;
                    else
                        ShowInfo = false;
                }

                //ButtonHeight = (int)position.width;
            }
            else
            {
                //ButtonMessage = "DISABLED";
                ShowInfo = false;
            }
        }

        [InitializeOnLoadAttribute]
        static class PlayModeStateChanged
        {
            static PlayModeStateChanged()
            {
                EditorApplication.playModeStateChanged += playModes;
                ButtonMessage = "DISABLED";
            }

            private static void playModes(PlayModeStateChange state)
            {
                if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode || state == UnityEditor.PlayModeStateChange.EnteredEditMode)
                {
                    //ShowInfo = false;
                    ButtonMessage = "DISABLED";
                }
                else
                {
                    //ShowInfo = true;
                    ButtonMessage = "READY TO REC";
                }
            }
        }

    }
#else
public class PerformanceAnimationReplacementManager : MonoBehaviour {

}
#endif
}