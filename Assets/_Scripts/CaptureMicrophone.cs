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
        private MrPuppetDataMapper DataMapper;

        [ReadOnly, ShowInInspector, ShowIf("ListenForCommands")]
        private bool UnityIsRecording = false;

        [ReadOnly, ShowInInspector, ShowIf("ListenForCommands")]
        private bool RecordingIsActive = false;

        [DisplayAsString, ShowInInspector, ShowIf("ListenForCommands")]
        private string Filename;

        private void Awake()
        {
            HubConnection = FindObjectOfType<MrPuppetHubConnection>();
            DataMapper = FindObjectOfType<MrPuppetDataMapper>();
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

            UnityIsRecording = Recorder.IsRecording();

            if (UnityIsRecording && !RecordingIsActive)
            {
                GetFilename();
                HubConnection.SendSocketMessage("COMMAND;START_MICROPHONE;" + Filename);

                HubConnection.SendSocketMessage("COMMAND;TPOSE;" + DataMapper.TPose.ToString());
                HubConnection.SendSocketMessage("COMMAND;JAW_OPENED;" + DataMapper.JawOpened);
                HubConnection.SendSocketMessage("COMMAND;JAW_CLOSED;" + DataMapper.JawClosed);
                HubConnection.SendSocketMessage("COMMAND;ATTACH;" + FindObjectOfType<ButtPuppet>().AttachPoseToString());
                // TODO: COMMAND;ARM_LENGTH
                // TODO: COMMAND;FOREARM_LENGTH

                RecordingIsActive = true;
            }
            else if (RecordingIsActive && !UnityIsRecording)
            {
                // DO NOT requery filename here as the take has already been advanced
                HubConnection.SendSocketMessage("COMMAND;STOP_MICROPHONE;" + Filename);
                RecordingIsActive = false;
            }
        }
#endif
    }
}
