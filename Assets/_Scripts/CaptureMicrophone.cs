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

        private bool IsRecording = false;

        private enum MicrophoneState
        {
            Idle,
            Recording,
            Stopped,
        };

        private bool RecordingMicrophone = false;

        private void FindRecorderWindow()
        {
            if (!Recorder) Recorder = EditorWindow.GetWindow<RecorderWindow>();
        }

        private void Update()
        {
            FindRecorderWindow();
            IsRecording = Recorder.IsRecording();

            if (IsRecording && !RecordingMicrophone)
            {
                Debug.Log("START MICROPHONE");
                RecordingMicrophone = true;
            }
            else if (RecordingMicrophone && !IsRecording)
            {
                Debug.Log("STOP MICROPHONE");
                RecordingMicrophone = false;
            }
        }
#endif
    }
}