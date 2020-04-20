
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;


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

        private Dictionary<int, float> FacsData;
        private float Timer;
        private string InfoBoxMsg = "Waiting for Take name... \n\nPlease select the actor in the scene";

        [InfoBox("Will play an audio file with the given filename in ~/Downloads in sync with recording. \n\nFor example: to play FC_day1.aif in sync with recording, place the FC_day1.aif file in ~/Downloads, enter \"FC_day1\" as the filename, and start the `yarn run ableton` link script.")]
        [OnValueChanged("LoadFACs")]
        public string Take;

        public string TestDataPath = "/Volumes/GoogleDrive/My Drive/Thinko/Shows/";

        [ToggleLeft]
        public bool EnableAudioPlayback = true;
        [ToggleLeft]
        public bool EnableFACSPlayback;

        [BoxGroup]
        [ShowIf("EnableFACSPlayback")]
        [InfoBox("$InfoBoxMsg")]
        [OnValueChanged("LoadFACs")]
        public GameObject Actor;

        private void LoadFACs()
        {
            FacsData = new Dictionary<int, float>();

            // var filePath = @"/Users/melindalastyak/HyperMesh/DOJO-E029-A001.txt";
            // /Volumes/GoogleDrive/My Drive/Shows/DOJO/episode/E029/performance/DOJO-E029-A001.txt

            var parts = new List<string>();
            if (Take.Contains('-'))
                parts = Take.Split('-').ToList();

            string filePath;
            if (parts.Count > 1)
                filePath = TestDataPath + parts[0] + "/episode/" + parts[1] + "/performance/" + Take + ".txt";
            else
                filePath = "/Volumes/GoogleDrive/My Drive/Thinko/Shows/" + "temp" + "/episode/" + "temp2" + "/performance/" + Take + ".txt";

            if (File.Exists(filePath))
            {
                var data = File.ReadLines(filePath).Skip(21).Select(x => x.Split(',')).ToArray();

                for (int i = 1; i < data.Count(); i++)
                {
                    FacsData.Add(int.Parse(data[i][1]), float.Parse(data[i][36]));
                }
                InfoBoxMsg = "Found " + Take + ".txt, loaded " + FacsData.Keys.Last() + " frames.";

                Timer = 0;
            }
            else
            {
                if (Take == "")
                    InfoBoxMsg = "Waiting for Take name...";
                else
                    InfoBoxMsg = "Could not find " + Take + ".txt";
            }

            if (Actor == null)
                InfoBoxMsg += "\n\nPlease select the actor in the scene.";
        }

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
                    if (EnableAudioPlayback == true)
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
                            LoadFACs();
                            TakeAfterPlay = Take;
                            HubConnection.SendSocketMessage("COMMAND;PLAYBACK;START;" + TakeAfterPlay);
                            AudioIsPlaying = true;
                            //}
                            //else
                            //  Debug.Log("Could not find the audio file associated with Audio Reference Take");
                        }
                    }

                    if (EnableFACSPlayback == true)
                    {
                        if (!Actor.GetComponent<JawTransformMapper>().UseJawPercentOverride)
                            Actor.GetComponent<JawTransformMapper>().UseJawPercentOverride = true;

                        Timer += Time.deltaTime * 1000;

                        foreach (KeyValuePair<int, float> item in FacsData)
                        {
                            if (Timer >= item.Key)
                            {
                                continue;
                            }

                            DebugGraph.Log(item.Value);

                            if (Actor != null)
                                Actor.GetComponent<JawTransformMapper>().JawPercentOverride = item.Value;

                            // TODO: Better way to check when animation is over
                            // Jacob: "frame loop you can store the value of the last timestamp. then you know once your time accumulation is >= that its done"
                            if (item.Key == FacsData.Keys.Last())
                            {
                                Timer = 0;
                            }

                            break;
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

                    if (Actor.GetComponent<JawTransformMapper>().UseJawPercentOverride)
                        Actor.GetComponent<JawTransformMapper>().UseJawPercentOverride = false;
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

                if (Actor != null)
                {
                    if (Actor.GetComponent<JawTransformMapper>().UseJawPercentOverride)
                        Actor.GetComponent<JawTransformMapper>().UseJawPercentOverride = false;
                }
            }
        }
    }
#else
    public class AnimationPlayback : MonoBehaviour
    {
    }
#endif
}
