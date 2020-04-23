
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
        private JawTransformMapper _JawTransformMapper;

        [InfoBox("Play an audio file in sync with recordings. \nEnter the performance name, and choose if you want audio and/or FaceCap playback. e.g. DOJO-E012 will attempt to play DOJO/episode/E012/performances/DOJO-E012.wav, .aif, .txt")]
        [OnValueChanged("LoadFACs")]
        [OnValueChanged("CacheJawTransformMapper")]
        public string Take;

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

            var settings = AssetDatabase.LoadAssetAtPath<MrPuppetSettings>("Assets/__Config/MrPuppetSettings.asset");

            string filePath = "";
            if (settings != null)
            {
                var parts = new List<string>();
                if (Take.Contains('-'))
                    parts = Take.Split('-').ToList();

                if (parts.Count > 1)
                    filePath = settings.ShowsRootPath + parts[0] + "/episode/" + parts[1] + "/performance/" + Take + ".txt";
            }
            else
            {
                MrPuppetSettings.GetOrCreateSettings();
            }

            if (File.Exists(filePath))
            {
                var data = File.ReadLines(filePath).Select(x => x.Split(',')).ToArray();

                int JawOpenIndex = new int();
                for (int i = 1; i < data.GetLength(0); i++)
                {
                    if (data[i][0] == "bs")
                    {
                        for (int x = 1; x < data.Count(); x++)
                        {
                            if (data[i][x] == "jawOpen")
                            {
                                JawOpenIndex = x + 11; // Add 11 to compensate for timestamp, head position, head eulerAngles, left-eye eulerAngles, right-eye eulerAngles
                                break;
                            }
                        }
                    }
                    if (data[i][0] == "k")
                    {
                        FacsData.Add(int.Parse(data[i][1]), float.Parse(data[i][JawOpenIndex]));
                    }
                }
                InfoBoxMsg = "Found " + Take + ".txt, loaded " + FacsData.Count + " frames.";
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

                if (!_JawTransformMapper)
                    CacheJawTransformMapper();

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
                        if (_JawTransformMapper && !_JawTransformMapper.UseJawPercentOverride)
                            _JawTransformMapper.UseJawPercentOverride = true;

                        Timer += Time.deltaTime * 1000f;

                        if (FacsData == null)
                            LoadFACs();

                        foreach (KeyValuePair<int, float> item in FacsData)
                        {
                            if (Timer >= item.Key)
                            {
                                continue;
                            }

                            if (_JawTransformMapper)
                                _JawTransformMapper.JawPercentOverride = item.Value;

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

                    if (_JawTransformMapper && _JawTransformMapper.UseJawPercentOverride)
                        _JawTransformMapper.UseJawPercentOverride = false;
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

                if (_JawTransformMapper && _JawTransformMapper.UseJawPercentOverride)
                    _JawTransformMapper.UseJawPercentOverride = false;
            }
        }

        private void CacheJawTransformMapper()
        {
            _JawTransformMapper = null;
            if (Actor != null)
            {
                if (Actor.GetComponent<JawTransformMapper>())
                    _JawTransformMapper = Actor.GetComponent<JawTransformMapper>();
            }
        }
    }
#else
    public class AnimationPlayback : MonoBehaviour
    {
    }
#endif
}
