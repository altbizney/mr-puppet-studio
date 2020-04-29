
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

        private float Timer;
        private string InfoBoxMsg = "Waiting for Take name... \n\nPlease select the actor in the scene";
        private JawTransformMapper _JawTransformMapper;
        private FaceCapBlendShapeMapper _FaceCapBlendShapeMapper;

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
        [OnValueChanged("CacheFaceCapBlendShapeMapper")]
        public GameObject Actor;

        private List<List<string>> FACSData = new List<List<string>>();
        private List<string> FACS_bs = new List<string>();
        private List<List<float>> FACS_k = new List<List<float>>();


        private void LoadFACs()
        {
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
                // clear old data
                FACS_bs.Clear();
                FACS_k.Clear();

                // load txt file, split each line into csv
                var Lines = File.ReadLines(filePath).Select(x => x.Split(',')).ToArray();

                // step thru every line
                for (int y = 0; y < Lines.Length; y++)
                {
                    // 0 is the string "info", "bs", "k"
                    // ignore info lines
                    if (Lines[y][0] == "info") continue;
                    // load bs lines
                    if (Lines[y][0] == "bs")
                    {
                        // remainder of line is array of channel names
                        FACS_bs = Lines[y].ToList();
                    }

                    var tempList = new List<float>();

                    if (Lines[y][0] == "k")
                    {
                        // 0 is k
                        // 1 is timestamp
                        // 2 to 12 are eulers
                        // remainder are blendshape values
                        tempList.Add(float.Parse(Lines[y][1]));

                        for (int x = 12; x < Lines[y].Length; x++)
                        {
                            tempList.Add(float.Parse(Lines[y][x]));
                        }

                        FACS_k.Add(tempList);
                    }
                }

                InfoBoxMsg = "Found " + Take + ".txt, loaded " + FACS_k.Count + " frames.";
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

        /*
        [Button(ButtonSizes.Small)]
        private void Print()
        {
            for (int y = 0; y < FACS_k.Count; y++)
            {
                for (int x = 0; x < FACS_k[y].Count; x++)
                {
                    Debug.Log(FACS_k[y][x]);
                }
            }

            Debug.Log("Length k " + FACS_k[0].Count); //53
            Debug.Log("Length bs " + FACS_bs.Count); //52

            //DOJO-E029-A001
            //TEST-E000-stub2
        }
        */

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

                if (!_FaceCapBlendShapeMapper)
                    CacheFaceCapBlendShapeMapper();

                if (Recorder.IsRecording())
                {
                    if (EnableAudioPlayback == true)
                    {
                        if (AudioIsPlaying == false)
                        {
                            LoadFACs();
                            TakeAfterPlay = Take;
                            HubConnection.SendSocketMessage("COMMAND;PLAYBACK;START;" + TakeAfterPlay);
                            AudioIsPlaying = true;
                        }
                    }

                    if (EnableFACSPlayback == true)
                    {

                        if (_JawTransformMapper && !_JawTransformMapper.UseJawPercentOverride)
                            _JawTransformMapper.UseJawPercentOverride = true;

                        Timer += Time.deltaTime * 1000f;

                        if (!FACSData.Any()) // facs.any
                            LoadFACs();

                        MapFACs();
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

        private void MapFACs()
        {
            for (int y = 0; y < FACS_k.Count; y++)
            {
                if (Timer >= FACS_k[y][0])
                    continue;

                bool found = false;
                for (int x = 0; x < FACS_k[y].Count(); x++)
                {
                    if (FACS_bs[x] == "jawOpen")
                    {
                        if (_JawTransformMapper)
                            _JawTransformMapper.JawPercentOverride = FACS_k[y][x];

                        found = true;
                    }

                    foreach (FaceCapBlendShapeMapper.BlendShapeMap map in _FaceCapBlendShapeMapper.Mappings)
                    {
                        if (map.Channel.ToString() == FACS_bs[x])
                        {
                            map._SkinnedMeshRenderer.SetBlendShapeWeight(map.BlendShape, FACS_k[y][x]);
                            found = true;
                        }
                    }
                }

                if (float.Parse(FACSData[y][0]) >= FACS_k.Count)
                    Timer = 0;

                //Found is probably not neccesary
                //Youll found no matter what.
                if (found == true)
                    return;
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

        public void CacheFaceCapBlendShapeMapper()
        {
            _FaceCapBlendShapeMapper = null;
            if (Actor)
            {
                if (Actor.GetComponent<FaceCapBlendShapeMapper>())
                    _FaceCapBlendShapeMapper = Actor.GetComponent<FaceCapBlendShapeMapper>();
            }
        }
    }
#else
    public class AnimationPlayback : MonoBehaviour
    {
    }
#endif
}
