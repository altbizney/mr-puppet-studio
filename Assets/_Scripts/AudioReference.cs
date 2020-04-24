
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections;

using System;

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

        private Dictionary<int, float> JawFacsData;
        private float Timer;
        private string InfoBoxMsg = "Waiting for Take name... \n\nPlease select the actor in the scene";
        private JawTransformMapper _JawTransformMapper;
        private static ValueDropdownList<SkinnedMeshRenderer> SkinnedMeshRenderers = new ValueDropdownList<SkinnedMeshRenderer>();


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

        [BoxGroup]
        [ShowIf("EnableFACSPlayback")]
        public List<Mapping> Mappings = new List<Mapping>();

        private string[][] FACSData;

        [Serializable]
        public class Mapping
        {
            public enum FACSChannels
            {
                browInnerUp,
                browDown_L,
                browDown_R,
                browOuterUp_L,
                browOuterUp_R,
                eyeLookUp_L,
                eyeLookUp_R,
                eyeLookDown_L,
                eyeLookDown_R,
                eyeLookIn_L,
                eyeLookIn_R,
                eyeLookOut_L,
                eyeLookOut_R,
                eyeBlink_L,
                eyeBlink_R,
                eyeSquint_L,
                eyeSquint_R,
                eyeWide_L,
                eyeWide_R,
                cheekPuff,
                cheekSquint_L,
                cheekSquint_R,
                noseSneer_L,
                noseSneer_R,
                jawOpen,
                jawForward,
                jawLeft,
                jawRight,
                mouthFunnel,
                mouthPucker,
                mouthLeft,
                mouthRight,
                mouthRollUpper,
                mouthRollLower,
                mouthShrugUpper,
                mouthShrugLower,
                mouthClose,
                mouthSmile_L,
                mouthSmile_R,
                mouthFrown_L,
                mouthFrown_R,
                mouthDimple_L,
                mouthDimple_R,
                mouthUpperUp_L,
                mouthUpperUp_R,
                mouthLowerDown_L,
                mouthLowerDown_R,
                mouthPress_L,
                mouthPress_R,
                mouthStretch_L,
                mouthStretch_R,
                tongueOut
            };
            public ValueDropdownList<int> _BlendShapeNames = new ValueDropdownList<int>();
            private ValueDropdownList<SkinnedMeshRenderer> _SkinnedMeshRenderers = new ValueDropdownList<SkinnedMeshRenderer>();



            [ValueDropdown("_BlendShapeNames")] //showif
            public int BlendShape;
            public FACSChannels Channel;
            [ValueDropdown("_SkinnedMeshRenderers")]
            public SkinnedMeshRenderer _SkinnedMeshRenderer;


            public Mapping()
            {
                _SkinnedMeshRenderers = AudioReference.SkinnedMeshRenderers;
            }


            [Button(ButtonSizes.Small)]
            public void GetBlendShapeNames()
            {
                _BlendShapeNames.Clear();

                for (var i = 0; i < _SkinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                {
                    _BlendShapeNames.Add(_SkinnedMeshRenderer.sharedMesh.GetBlendShapeName(i), i);
                }
            }
        }

        //[Button(ButtonSizes.Large)]
        public void GetSkinnedMeshRenderers()
        {
            SkinnedMeshRenderers.Clear();
            if (Actor)
            {
                foreach (Transform child in Actor.GetComponentsInChildren<Transform>())
                {
                    if (child.gameObject.GetComponent<SkinnedMeshRenderer>())
                    {
                        SkinnedMeshRenderer childMesh = child.gameObject.GetComponent<SkinnedMeshRenderer>();
                        if (childMesh.sharedMesh.blendShapeCount > 0)
                            SkinnedMeshRenderers.Add(childMesh);
                    }
                }
            }
        }

        private void LoadFACs()
        {
            JawFacsData = new Dictionary<int, float>();

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
                FACSData = File.ReadLines(filePath).Skip(21).Select(x => x.Split(',')).ToArray();

                int JawOpenIndex = new int();
                for (int i = 1; i < FACSData.GetLength(0); i++)
                {
                    if (FACSData[i][0] == "bs")
                    {
                        for (int x = 1; x < FACSData.Count(); x++)
                        {
                            if (FACSData[i][x] == "jawOpen")
                            {
                                JawOpenIndex = x + 11; // Add 11 to compensate for timestamp, head position, head eulerAngles, left-eye eulerAngles, right-eye eulerAngles
                                break;
                            }
                        }
                    }
                    if (FACSData[i][0] == "k")
                    {
                        //FacsData.Add(int.Parse(FACSData[i][1]), float.Parse(FACSData[i][JawOpenIndex]));
                    }
                }
                InfoBoxMsg = "Found " + Take + ".txt, loaded " + JawFacsData.Count + " frames.";
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

                foreach (Mapping map in Mappings)
                {
                    if (!map._BlendShapeNames.Any())
                        map.GetBlendShapeNames();
                }

                if (!SkinnedMeshRenderers.Any())
                    GetSkinnedMeshRenderers();

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

                        if (FACSData == null) //if jawfacsdata == null
                            LoadFACs();

                        /*
                        foreach (KeyValuePair<int, float> item in FacsData)
                        {
                            if (Timer >= item.Key)
                            {
                                continue;
                            }

                            //if (_JawTransformMapper)
                            //    _JawTransformMapper.JawPercentOverride = item.Value;
                            //skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, output);

                            foreach (Mapping map in Mappings)
                            {
                                map._SkinnedMeshRenderer.SetBlendShapeWeight(map.BlendShape, item.Value * 100);
                            }

                            // TODO: Better way to check when animation is over
                            // Jacob: "frame loop you can store the value of the last timestamp. then you know once your time accumulation is >= that its done"
                            if (item.Key == FacsData.Keys.Last())
                            {
                                Timer = 0;
                            }

                            break;
                        }
                        */

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
            for (int y = 1; y < FACSData.GetLength(0); y++)
            {
                if (Timer >= float.Parse(FACSData[y][1]))
                    continue;

                for (int x = 0; x < FACSData.Count(); x++)
                {
                    bool found = false;
                    foreach (Mapping map in Mappings)
                    {
                        //Debug.Log(FACSData[0][x]);
                        if (map.Channel.ToString() == FACSData[0][x])
                        {
                            map._SkinnedMeshRenderer.SetBlendShapeWeight(map.BlendShape, float.Parse(FACSData[y][x + 11]) * 100f);
                            //Debug.Log(FACSData[y][1] + " " + FACSData[0][x] + " " + FACSData[y][x + 11]);
                            found = true;
                        }
                    }
                    if (found == true)
                        return;
                }
            }
            //Tell when animation is over
            //Reset timer
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
