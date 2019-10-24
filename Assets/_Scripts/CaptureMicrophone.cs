using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Recorder;
#endif

namespace MrPuppet
{
    public class CaptureMicrophone : MonoBehaviour
    {
#if UNITY_EDITOR
        private RecorderWindow Recorder;

        private MrPuppetHubConnection HubConnection;

        private bool IsRecording = false;

        private bool RecordingMicrophone = false;

        private string Filename;

        private void OnValidate()
        {
            if (HubConnection == null) HubConnection = FindObjectOfType<MrPuppetHubConnection>();
        }

        private void GetFilename()
        {
            RecorderControllerSettings m_ControllerSettings = RecorderControllerSettings.LoadOrCreate(Application.dataPath + "/../Library/Recorder/recorder.pref");
            RecorderController m_RecorderController = new RecorderController(m_ControllerSettings);

            foreach (var recorder in m_ControllerSettings.recorderSettings)
            {
                if (!recorder.enabled) continue;

                Filename = recorder.outputFile;

                Filename = Filename.Replace("<Take>", recorder.take.ToString("000"));
                Filename = Filename.Replace("<Scene>", SceneManager.GetActiveScene().name);

                Filename = Filename.Substring(Filename.LastIndexOf('/') + 1);

                // just need the first
                return;
            }
        }

        private void Update()
        {
            if (!Recorder) Recorder = EditorWindow.GetWindow<RecorderWindow>();

            IsRecording = Recorder.IsRecording();

            if (IsRecording && !RecordingMicrophone)
            {
                GetFilename();
                HubConnection.SendSocketMessage("COMMAND;START_MICROPHONE;" + Filename);
                RecordingMicrophone = true;
            }
            else if (RecordingMicrophone && !IsRecording)
            {
                // DO NOTT requery filename here as the take has already been advanced
                HubConnection.SendSocketMessage("COMMAND;STOP_MICROPHONE;" + Filename);
                RecordingMicrophone = false;
            }
        }
#endif
    }
}