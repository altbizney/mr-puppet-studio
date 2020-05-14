using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Collections;


//#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Recorder;
using Sirenix.OdinInspector;
//using Sirenix.OdinInspector.Editor;
//#endif

namespace MrPuppet
{
//#if UNITY_EDITOR

    [ExecuteInEditMode] 
    public class AudioReference : MonoBehaviour
    {
        /*[MenuItem("Tools/Audio Reference")]
        private static void OpenWindow()
        {
            GetWindow<AudioReference>().Show();
        }*/

        private MrPuppetHubConnection HubConnection;
        private RecorderWindow Recorder;
        private JawTransformMapper _JawTransformMapper;
        private FaceCapBlendShapeMapper _FaceCapBlendShapeMapper;

        private double Timer;
        private double TimerOffset;

        private string TakeAfterPlay;
        private string InfoBoxMsg = "Waiting for Take name... ";//\n\nPlease select the actor in the scene
        private bool DisablePlayButton;
        private bool PlayModeEntered = false;
        private bool FACSPlayButton;
        private bool AudioIsPlaying;
        private bool RecorderStartedRecording;

        //[SerializeField]
        private List<FaceCapBlendShapeMapper.BlendShapeMap.FACSChannels> FACS_bs = new List<FaceCapBlendShapeMapper.BlendShapeMap.FACSChannels>();

        [ListDrawerSettings(IsReadOnly = true)]
        private List<List<float>> FACS_k = new List<List<float>>();

        [InfoBox("Play an audio file in sync with recordings. \nEnter the performance name, and choose if you want audio and/or FaceCap playback. e.g. DOJO-E012 will attempt to play DOJO/episode/E012/performances/DOJO-E012.wav, .aif, .txt")]
        [OnValueChanged("LoadFACS")]
        [OnValueChanged("CacheJawTransformMapper")]
        [InfoBox("$InfoBoxMsg")]
        public string Take;

        [ToggleLeft]
        public bool EnableAudioPlayback = true;
        [ToggleLeft]
        public bool EnableFACSPlayback;

        /*[BoxGroup]
        [ShowIf("EnableFACSPlayback")]
        [OnValueChanged("LoadFACS")]
        [OnValueChanged("CacheFaceCapBlendShapeMapper")]
        public GameObject Actor;*/

        private bool NotPlaying()
        {
            return !FACSPlayButton;
        }

        private bool IsPlaying()
        {
            return FACSPlayButton;
        }

        [HideIf("IsPlaying", false)]
        [ShowIf("NotPlaying", false)]
        [DisableInEditorMode]
        [Button(ButtonSizes.Large)]
        [DisableIf("DisablePlayButton")]
        public void Play()
        {
            Timer = 0;//TimerOffset = EditorApplication.timeSinceStartup * 1000f;
            FACSPlayButton = true;
        }

        [HideIf("NotPlaying", false)]
        [ShowIf("IsPlaying", false)]
        [DisableInEditorMode]
        [Button(ButtonSizes.Large)]
        [DisableIf("DisablePlayButton")]
        public void Stop()
        {
            Timer = 0;
            //TimerOffset = Timer = 0f;
            FACSPlayButton = false;
            ResetBlendShapes();
        }

        private void ResetBlendShapes()
        {
            foreach (SkinnedMeshRenderer smr in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (smr.sharedMesh.blendShapeCount > 0)
                {
                    for (var i = 0; i < smr.sharedMesh.blendShapeCount; i++)
                    {
                        smr.SetBlendShapeWeight(i, 0);
                    }
                }
            }
        }

        private void Awake() {
            LoadFACS();
        }

        private void Start()
        {
            StartCoroutine(LateStart());
        }

        IEnumerator LateStart()
        {
            yield return new WaitForSeconds(0.25f);
            if (!string.IsNullOrEmpty(Take) && EnableAudioPlayback == true)
            {
                if (!HubConnection)
                    HubConnection = FindObjectOfType<MrPuppetHubConnection>();
                
                if(Application.isPlaying)
                    HubConnection.SendSocketMessage("COMMAND;PLAYBACK;LOAD;" + Take);
            }
        }

        private void PlaybackLogic()
        {
            if (!FACS_k.Any())
                LoadFACS();

            if (EnableAudioPlayback == true)
            {
                //Reset Blend Shapes when entering PlayMode?

                if (AudioIsPlaying == false)
                {
                    TakeAfterPlay = Take;
                    HubConnection.SendSocketMessage("COMMAND;PLAYBACK;START;" + TakeAfterPlay);
                    AudioIsPlaying = true;
                }
            }

            if (EnableFACSPlayback == true)
            {

                if (_JawTransformMapper && !_JawTransformMapper.UseJawPercentOverride)
                    _JawTransformMapper.UseJawPercentOverride = true;

                //Timer = EditorApplication.timeSinceStartup * 1000f;
                Timer += Time.deltaTime * 1000f;

                for (int k = 0; k < FACS_k.Count; k++)
                {

                    //Stop recording when the time since we started recording is greater than the last keyframe
                    if (Timer  >= FACS_k[FACS_k.Count - 1][0])
                    {
                        //Timer = 0;
                        Stop();

                        if (Recorder.IsRecording())
                            Recorder.StopRecording();

                        break;
                    }                    

                    //Don't continue to the bottom of the loop, until we get a value bigger than timer.
                    //This guarantees we get the next biggest value after all the values smaller then us.
                    //This logic allows us to drop frames when needed.
                    if (Timer >= FACS_k[k][0])
                        continue;
                    
                    //Go through the list of channels
                    //When the channel we reached in our List matches either jawOpen or a channel found within the list of Mappings inside of the FaceCapBlendShapeMapper component
                    //we take the corresponding value associated with that channel at that timestamp
                    //and apply it to either a value within JawTransformMapper, or to the corresponding BlendShape dictated by FaceCapeBlendShapeMapper
                    //We must provide an offset to bs because of the difference in size of the Lists
                    for (int bs = 0; bs < FACS_bs.Count(); bs++)
                    {
                        if (FACS_bs[bs] == FaceCapBlendShapeMapper.BlendShapeMap.FACSChannels.jawOpen)
                        {
                            if (_JawTransformMapper)
                            {
                                _JawTransformMapper.JawPercentOverride = FACS_k[k][bs + 1];
                            }
                        }

                        if (_FaceCapBlendShapeMapper && _FaceCapBlendShapeMapper.Mappings != null && _FaceCapBlendShapeMapper.Mappings.Count > 0)
                        {
                            foreach (FaceCapBlendShapeMapper.BlendShapeMap map in _FaceCapBlendShapeMapper.Mappings)
                            {
                                if (map.Channel == FACS_bs[bs])
                                    map._SkinnedMeshRenderer.SetBlendShapeWeight(map.BlendShape, FACS_k[k][bs + 1] * 100f);
                            }
                        }
                    }

                    //We are only interested in the next valid frame, and should only perform logic for said frame. A break ensures that.
                    break;
                }
            }
        }

        private void LoadFACS()
        {
            var settings = AssetDatabase.LoadAssetAtPath<MrPuppetSettings>("Assets/__Config/MrPuppetSettings.asset");

            string filePath = "";
            if (settings != null)
            {
                if(Take != null){
                    var parts = new List<string>();
                    if (Take.Contains('-'))
                        parts = Take.Split('-').ToList();

                    if (parts.Count > 1)
                        filePath = settings.ShowsRootPath + parts[0] + "/episode/" + parts[1] + "/performance/" + Take + ".txt";
                }
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
                        FACS_bs = Lines[y].Skip(1).ToList().ConvertAll(delegate (string x) { return (FaceCapBlendShapeMapper.BlendShapeMap.FACSChannels)Enum.Parse(typeof(FaceCapBlendShapeMapper.BlendShapeMap.FACSChannels), x); });
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

                if (EnableAudioPlayback && Application.isPlaying){   //&& EditorApplication.isPlaying
                    if (!HubConnection)
                        HubConnection = FindObjectOfType<MrPuppetHubConnection>();

                    HubConnection.SendSocketMessage("COMMAND;PLAYBACK;LOAD;" + Take);
                }
            }
            else
            {
                if (Take == "" || Take == null)
                    InfoBoxMsg = "Waiting for Take name...";
                else
                    InfoBoxMsg = "Could not find " + Take + ".txt";
            }
        }

        private void Update()
        {
            /*
            if (!Actor)
            {
                if (!InfoBoxMsg.Contains("Please select the actor in the scene"))
                    InfoBoxMsg += "\n\nPlease select the actor in the scene.";
            }
            */

            //if (EditorApplication.isPlaying)
           // {
                //Repaint();

                if (!Recorder)
                {
                    try { Recorder = EditorWindow.GetWindow<RecorderWindow>(); }
                    catch { return; }
                }

                if (!HubConnection)
                {
                    try { HubConnection = FindObjectOfType<MrPuppetHubConnection>(); }
                    catch { return; }
                }

                if (!Recorder || !HubConnection)
                    return;

                if (!_JawTransformMapper)
                    CacheJawTransformMapper();

                if (!_FaceCapBlendShapeMapper)
                    CacheFaceCapBlendShapeMapper();

                if (RecorderStartedRecording == true && !Recorder.IsRecording())
                {
                    RecorderStartedRecording = false;
                    ResetBlendShapes();
                }

                if (Recorder.IsRecording())
                {
                    if(!RecorderStartedRecording)
                    {
                        RecorderStartedRecording = true;
                        //Timer = 0;//TimerOffset = EditorApplication.timeSinceStartup * 1000f;
                    }

                    DisablePlayButton = true;
                    PlaybackLogic();
                }
                else
                    DisablePlayButton = false;

                if (FACSPlayButton)
                {
                    if (Recorder.IsRecording())
                    {
                        Stop();
                        if (AudioIsPlaying == true)
                        {
                            HubConnection.SendSocketMessage("COMMAND;PLAYBACK;STOP;" + TakeAfterPlay);
                            HubConnection.SendSocketMessage("COMMAND;PLAYBACK;LOAD;" + Take);
                            AudioIsPlaying = false;
                        }
                    }

                    PlaybackLogic();
                }

                if (!Recorder.IsRecording() && !FACSPlayButton)
                {
                    if (AudioIsPlaying == true)
                    {
                        Stop();
                        HubConnection.SendSocketMessage("COMMAND;PLAYBACK;STOP;" + TakeAfterPlay);
                        HubConnection.SendSocketMessage("COMMAND;PLAYBACK;LOAD;" + Take);
                        AudioIsPlaying = false;
                    }

                    if (_JawTransformMapper && _JawTransformMapper.UseJawPercentOverride)
                        _JawTransformMapper.UseJawPercentOverride = false;
                }
            //}
            /*
            if (!EditorApplication.isPlaying)
            {
                if (AudioIsPlaying == true)
                {
                    HubConnection.SendSocketMessage("COMMAND;PLAYBACK;STOP;" + TakeAfterPlay);
                    AudioIsPlaying = false;
                }

                if (_JawTransformMapper && _JawTransformMapper.UseJawPercentOverride)
                    _JawTransformMapper.UseJawPercentOverride = false;

                if (PlayModeEntered == true)
                    PlayModeEntered = false;
                if (FACSPlayButton == true)
                    FACSPlayButton = false;
                if (DisablePlayButton == true)
                    DisablePlayButton = false;
                if (RecorderStartedRecording == true)
                    RecorderStartedRecording = false;
                if (TakeAfterPlay != "")
                    TakeAfterPlay = "";
            }
            */
        }

        public void OnApplicationQuit(){
                if (AudioIsPlaying == true)
                {
                    HubConnection.SendSocketMessage("COMMAND;PLAYBACK;STOP;" + TakeAfterPlay);
                    AudioIsPlaying = false;
                }

                if (_JawTransformMapper && _JawTransformMapper.UseJawPercentOverride)
                    _JawTransformMapper.UseJawPercentOverride = false;

                if (PlayModeEntered == true)
                   PlayModeEntered = false;
                if (FACSPlayButton == true)
                    FACSPlayButton = false;
                if (DisablePlayButton == true)
                    DisablePlayButton = false;
                if (RecorderStartedRecording == true)
                    RecorderStartedRecording = false;
                if (TakeAfterPlay != "")
                    TakeAfterPlay = "";
                
                LoadFACS();
        }


        private void CacheJawTransformMapper()
        {
            _JawTransformMapper = null;
            //if (Actor != null)
            //{
                if (gameObject.GetComponent<JawTransformMapper>())
                    _JawTransformMapper = gameObject.GetComponent<JawTransformMapper>();
            //}
        }

        public void CacheFaceCapBlendShapeMapper()
        {
            _FaceCapBlendShapeMapper = null;
            //if (Actor)
            //{
                if (gameObject.GetComponent<FaceCapBlendShapeMapper>())
                    _FaceCapBlendShapeMapper = gameObject.GetComponent<FaceCapBlendShapeMapper>();
            //}
        }
    }
//#else
    //public class AnimationPlayback : MonoBehaviour
    //{ }
//#endif
}
