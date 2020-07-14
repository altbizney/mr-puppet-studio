using UnityEngine;
using UnityEditor.Recorder;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Recorder;
using UnityEditor.Recorder.Input;
using System.Linq;

//using UnityEngine.Recorder.Input;


#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor.Animations;
#endif

namespace MrPuppet
{
#if UNITY_EDITOR
    public class OneShotsWindow : OdinEditorWindow
    {
        [MenuItem("Tools/One Shots Performance")]
        private static void OpenWindow()
        {
            GetWindow<OneShotsWindow>().Show();
        }

        [OnValueChanged("ParseAnimationClip")]
        public AnimationClip Performance;

        [OnValueChanged("ParseAnimationClip")]
        public bool IncludeAnimationSuffix;
        
        [ReadOnly]
        public string ParsedClipName;

        public GameObject Actor;
        private RecorderWindow Recorder;
        private bool StartedRecording;
        private MrPuppetHubConnection HubConnection;
        private bool PlayModeEntered;

        private void Update()
        {
            if (EditorApplication.isPlaying)
            {
                if (PlayModeEntered == false)
                {
                    if (!string.IsNullOrEmpty(ParsedClipName) && ParsedClipName.Contains("-"))
                    {
                        if (HubConnectionCheck())
                        {
                            HubConnection.SendSocketMessage("COMMAND;PLAYBACK;LOAD;" + ParsedClipName);
                        }
                    }
                    PlayModeEntered = true;
                }

                if (!Recorder.IsRecording() && StartedRecording == true)
                    EditorApplication.isPlaying = false;
            }
            else
            {
                if (PlayModeEntered == true)
                    PlayModeEntered = false;
                
                StartedRecording = false;
            }
        }

        [Button("Play")]
        [DisableInEditorMode]
        private void Action()
        { 
            GameObject Clone = Instantiate(Actor, new Vector3(0, 2f, 3f), Actor.transform.rotation);

            if (HubConnectionCheck()){
                HubConnection.SendSocketMessage("COMMAND;PLAYBACK;START;" + ParsedClipName);
            }

            Actor.SetActive(false);

            KillChildren(Clone.GetComponentsInChildren<MrPuppet.JawTransformMapper>());
            KillChildren(Clone.GetComponentsInChildren<MrPuppet.ButtPuppet>());
            KillChildren(Clone.GetComponentsInChildren<MrPuppet.IKButtPuppet>());
            KillChildren(Clone.GetComponentsInChildren<MrPuppet.Blink>());
            KillChildren(Clone.GetComponentsInChildren<MrPuppet.HeadPuppet>());
            KillChildren(Clone.GetComponentsInChildren<MrPuppet.JawBlendShapeMapper>());
            KillChildren(Clone.GetComponentsInChildren<MrPuppet.JointFollower>());
            KillChildren(Clone.GetComponentsInChildren<MrPuppet.LookAtTarget>());
            KillChildren(Clone.GetComponentsInChildren<MrPuppet.CaptureMicrophone>());
            KillChildren(Clone.GetComponentsInChildren<OneShotAnimations>());
            KillChildren(Clone.GetComponentsInChildren<IKTag>());
            KillChildren(Clone.GetComponentsInChildren<Animator>());
            //There are more components on the clone that are potentially unnessary

            Animator AnimatorTemplate = Clone.AddComponent<Animator>(); 

            AssetDatabase.CopyAsset("Assets/Resources/OneShots.controller", "Assets/Resources/OneShotsTemp.controller");
            AnimatorTemplate.runtimeAnimatorController =  Resources.Load("OneShotsTemp") as RuntimeAnimatorController;

            AnimatorOverrideController AnimatorOverride = new AnimatorOverrideController(AnimatorTemplate.runtimeAnimatorController);
            AnimatorTemplate.runtimeAnimatorController = AnimatorOverride;
            AnimatorOverride["BaseAnimation"] = Performance;
            Clone.AddComponent<OneShotTesting>(); 

            if (Recorder == null)
                Recorder = EditorWindow.GetWindow<RecorderWindow>();

            Recorder.StartRecording();
            StartedRecording = true;
            SetRecorderTarget(Clone);
        }

        private void KillChildren(UnityEngine.Object[] children)
        {
            foreach (UnityEngine.Object child in children)
                Destroy(child);
        }

        private void SetRecorderTarget(GameObject Clone)
        {
            RecorderControllerSettings m_ControllerSettings = RecorderControllerSettings.LoadOrCreate(Application.dataPath + "/../Library/Recorder/recorder.pref");
            RecorderController m_RecorderController = new RecorderController(m_ControllerSettings);

            foreach (var recorder in m_RecorderController.Settings.RecorderSettings)
            {
                if (!recorder.Enabled) continue;

                foreach (var input in recorder.InputsSettings) { ((AnimationInputSettings)input).gameObject = Clone; }

                return;
            }
        }

        private bool HubConnectionCheck()
        {   
            //Check to see if name is correct format?

            if (HubConnection != FindObjectOfType<MrPuppetHubConnection>() || !HubConnection)
                HubConnection = FindObjectOfType<MrPuppetHubConnection>();

            if (HubConnection)
                return true;

            return false;
        }

        
        private void ParseAnimationClip()
        {
            if (Performance)
            {
                if (Performance.name.Count(c => c == '-') == 2)
                {
                    if (!IncludeAnimationSuffix)
                    {
                        ParsedClipName = Performance.name.Substring(0, Performance.name.LastIndexOf("-"));
                    }
                    else
                    {
                        ParsedClipName = Performance.name;
                    }

                    if (HubConnectionCheck() && EditorApplication.isPlaying)
                        HubConnection.SendSocketMessage("COMMAND;PLAYBACK;LOAD;" + ParsedClipName);
                }
                else
                    ParsedClipName = "Error: Format";
            }
        }

        
        [InitializeOnLoadAttribute]
        static class PlayModeStateChanged
        {
            static PlayModeStateChanged()
            {
                EditorApplication.playModeStateChanged += playModes;
            }

            private static void playModes(PlayModeStateChange state)
            {

                if (state == UnityEditor.PlayModeStateChange.EnteredEditMode)
                {
                    AssetDatabase.DeleteAsset("Assets/Resources/OneShotsTemp.controller");
                }
            }
        }

    }
#else
    public class PerformanceAnimationReplacementManager : MonoBehaviour
    {

    }
#endif
}
