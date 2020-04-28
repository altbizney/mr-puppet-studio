
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
                var TempData = File.ReadLines(filePath).Select(x => x.Split(',')).ToArray();

                FACSData.Clear();

                for (int y = 0; y < TempData.Length; y++)
                {
                    if (TempData[y][0] != "info")
                    {
                        var tempList = new List<string>();
                        for (int x = 0; x < TempData[y].Length; x++)
                        {
                            // TODO: Cache .ToString, etc.
                            // Load 'bs' into its own array?
                            if (TempData[y][0] == "k")
                            {
                                if (!(x > 1 && x <= 12))
                                    tempList.Add(TempData[y][x]);
                            }
                            else
                                tempList.Add(TempData[y][x]);
                        }
                        FACSData.Add(tempList);
                    }
                }

                InfoBoxMsg = "Found " + Take + ".txt, loaded " + FACSData.Count + " frames.";
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
            for (int y = 1; y < FACSData.Count; y++)
            {
                if (Timer >= float.Parse(FACSData[y][1]))
                    continue;

                bool found = false;
                for (int x = 0; x < FACSData[0].Count(); x++)
                {
                    if (FACSData[0][x] == "jawOpen")
                    {
                        if (_JawTransformMapper)
                            _JawTransformMapper.JawPercentOverride = float.Parse(FACSData[y][x]);
                        found = true;
                    }

                    foreach (FaceCapBlendShapeMapper.BlendShapeMap map in _FaceCapBlendShapeMapper.Mappings)
                    {
                        if (map.Channel.ToString() == FACSData[0][x])
                        {
                            map._SkinnedMeshRenderer.SetBlendShapeWeight(map.BlendShape, float.Parse(FACSData[y][x]) * 100f);
                            found = true;
                        }
                    }
                }
                if (found == true)
                    return;
            }
            // TODO: Detect end of animation + Reset timer
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
