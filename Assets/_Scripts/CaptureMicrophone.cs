using Sirenix.OdinInspector;
using UnityEngine;
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
        public bool ListenForCommands = true;

        private RecorderWindow Recorder;
        private MrPuppetHubConnection HubConnection;

        [ReadOnly, ShowInInspector, ShowIf("ListenForCommands")]
        private bool IsRecording = false;

        [ReadOnly, ShowInInspector, ShowIf("ListenForCommands")]
        private bool RecordingMicrophone = false;

        [DisplayAsString, ShowInInspector, ShowIf("ListenForCommands")]
        private string Filename;

        private void Awake()
        {
            HubConnection = FindObjectOfType<MrPuppetHubConnection>();
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
            if (!ListenForCommands) return;

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
                // DO NOT requery filename here as the take has already been advanced
                HubConnection.SendSocketMessage("COMMAND;STOP_MICROPHONE;" + Filename);
                RecordingMicrophone = false;
            }

            // TODO: move these to where they happen
            if (Input.GetKeyDown(KeyCode.A))
                HubConnection.SendSocketMessage("COMMAND;KEYDOWN:A\n");
            if (Input.GetKeyDown(KeyCode.T))
                HubConnection.SendSocketMessage("COMMAND;KEYDOWN:T\n");
            if (Input.GetKeyDown(KeyCode.O))
                HubConnection.SendSocketMessage("COMMAND;KEYDOWN:O\n");
            if (Input.GetKeyDown(KeyCode.C))
                HubConnection.SendSocketMessage("COMMAND;KEYDOWN:C\n");
        }
#endif
    }
}
