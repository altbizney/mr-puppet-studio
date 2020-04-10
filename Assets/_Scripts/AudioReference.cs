
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor.Recorder;
#endif

namespace MrPuppet
{
#if UNITY_EDITOR
    public class AudioReference : OdinEditorWindow
    {
        [MenuItem("Tools/Audio Reference")]
        private static void OpenWindow()
        {
            GetWindow<AudioReference>().Show();
        }

        private MrPuppetHubConnection HubConnection;
        private RecorderWindow Recorder;
        private bool AudioIsPlaying;
        private string TakeAfterPlay;

        [InfoBox("Will play an audio file with the given filename in ~/Downloads in sync with recording. \n\nFor example: to play FC_day1.aif in sync with recording, place the FC_day1.aif file in ~/Downloads, enter \"FC_day1\" as the filename, and start the `yarn run ableton` link script.")]
        public string Take;
        public bool EnablePlayback = true;

        private void Update()
        {
            if (EditorApplication.isPlaying)
            {
                if (!Recorder)
                {
                    try
                    { Recorder = EditorWindow.GetWindow<RecorderWindow>(); }
                    catch
                    { return; }
                }

                if (!HubConnection)
                {
                    try
                    { HubConnection = FindObjectOfType<MrPuppetHubConnection>(); }
                    catch
                    { return; }
                }

                if (!Recorder || !HubConnection)
                    return;

                if (Recorder.IsRecording())
                {
                    if (EnablePlayback == true)
                    {
                        if (AudioIsPlaying == false)
                        {
                            //bool found = false;
                            //string DataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/Downloads";
                            //DirectoryInfo d = new DirectoryInfo(DataPath);
                            //FileInfo[] Files = d.GetFiles("*.wav");
                            //foreach (FileInfo file in Files)
                            //{
                                //  if (file.Name == Take.ToUpper() + ".wav")
                                //       found = true;
                            //}

                            //if (found == true)
                            //{
                                TakeAfterPlay = Take;
                                HubConnection.SendSocketMessage("COMMAND;PLAYBACK;START;" + TakeAfterPlay);
                                AudioIsPlaying = true;
                            //}
                            //else
                                //  Debug.Log("Could not find the audio file associated with Audio Reference Take");
                        }
                    }

                }

                if (!Recorder.IsRecording())
                {
                    if (AudioIsPlaying == true)
                    {
                        HubConnection.SendSocketMessage("COMMAND;PLAYBACK;STOP;" + TakeAfterPlay);
                        AudioIsPlaying = false;
                    }
                }
            }

            if (!EditorApplication.isPlaying)
            {
                if (AudioIsPlaying == true)
                {
                    HubConnection.SendSocketMessage("COMMAND;PLAYBACK;STOP;" + TakeAfterPlay);
                    AudioIsPlaying = false;
                }

                TakeAfterPlay = "";
            }
        }
    }
#else
    public class AnimationPlayback : MonoBehaviour
    {
    }
#endif
}
