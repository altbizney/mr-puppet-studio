using UnityEngine;
using Sirenix.OdinInspector;

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

        private void OnValidate()
        {
            if (HubConnection == null) HubConnection = FindObjectOfType<MrPuppetHubConnection>();
        }

        private void Update()
        {
            if (!Recorder) Recorder = EditorWindow.GetWindow<RecorderWindow>();

            IsRecording = Recorder.IsRecording();

            if (IsRecording && !RecordingMicrophone)
            {
                HubConnection.SendSocketMessage("COMMAND;START_MICROPHONE");
                RecordingMicrophone = true;
            }
            else if (RecordingMicrophone && !IsRecording)
            {
                HubConnection.SendSocketMessage("COMMAND;STOP_MICROPHONE");
                RecordingMicrophone = false;
            }
        }
#endif
    }
}