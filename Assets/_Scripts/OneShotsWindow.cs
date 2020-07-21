using UnityEngine;
using System.Collections;
using UnityEngine.Recorder;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor.Animations;
#endif

namespace MrPuppet
{
#if UNITY_EDITOR
    public class OneShotsWindow : OdinEditorWindow
    {
        //Audio Ref wont stop audio when mode changes yet. 
        //Add Repaint.
        //I think get window opens the window??? cant use oneshots without recorder window. 
        //if recorder helper is not open, it opens it. that is even kinda weird. 
        //How should all this interact with each other? can you use one without the other for everything?

        //animation clip not being found when using one shots normally? 

        [MenuItem("Tools/One Shots Performance")]
        private static void OpenWindow()
        {
            GetWindow<OneShotsWindow>().Show();
        }

        [OnValueChanged("ParseAnimationClip")]
        public AnimationClip Performance;

        public GameObject Actor;

        [OnValueChanged("ParseAnimationClip")]
        [HorizontalGroup("Bottom")]
        public bool IncludeAnimationSuffix;
        
        [ReadOnly]
        [HorizontalGroup("Bottom")]
        public string ParsedClipName;

        private RecorderHelper ModeAccess;

        private RecorderWindow Recorder;
        private bool PlayModeEntered;
        private GameObject Clone;
        private Coroutine AnimationCoroutine;
        private GameObject CoroutineHolder;
        static private int TakeCount; //Consider adding in logic to remove static
        static private string RecordedName;
        static public string RecordedPaddedName;
        static private string ParsedClipNameAfterPlay;
        static private MrPuppetHubConnection HubConnection;

        private bool NotPlaying(){ return !Clone; }
        private bool IsPlaying(){ return Clone; }
        
        private bool IsAudRef()
        { 
            ModeAccess = (RecorderHelper)EditorWindow.GetWindow(typeof(RecorderHelper), false, null, false);

            if (ModeAccess.Mode == RecorderHelper.AudioModes.AudRef)
            {
                return true;
            }
            else
            {
              return false;
            }
        }
        
        public class BlankMonoBehaviour : MonoBehaviour{ }
        public class _OnDestroy : MonoBehaviour
        { 
            void OnDestroy() 
            { 
                //Debug.Log(AssetDatabase.RenameAsset("Assets/Recordings/" + OneShotsWindow.RecordedName + ".anim", OneShotsWindow.RecordedPaddedName + ".anim"));
                AssetDatabase.RenameAsset("Assets/Recordings/" + OneShotsWindow.RecordedName + ".anim", OneShotsWindow.RecordedPaddedName + ".anim");
                AssetDatabase.SaveAssets();
                OneShotsWindow.HubConnection.SendSocketMessage("COMMAND;PLAYBACK;STOP;" + OneShotsWindow.ParsedClipNameAfterPlay);
            } 
        }
        
        [HideIf("IsPlaying", false)]
        [ShowIf("NotPlaying", false)]
        [DisableIf("IsAudRef")]
        [DisableInEditorMode]
        [GUIColor(0.2f, 0.9f, 0.2f)]
        [Button(ButtonSizes.Large)]
        public void Record()
        { 
            if (IsAudRef() == false)
            {
                if (HubConnectionCheck()){
                    ParsedClipNameAfterPlay = ParsedClipName;
                    HubConnection.SendSocketMessage("COMMAND;PLAYBACK;START;" + ParsedClipName);
                }

                Clone = Instantiate(Actor, Actor.transform.position, Actor.transform.rotation);
                CoroutineHolder = new GameObject("CoroutineHolder");
                CoroutineHolder.AddComponent<BlankMonoBehaviour>();
                Clone.AddComponent<_OnDestroy>();

                Actor.SetActive(false);

                KillChildren(Clone.GetComponentsInChildren<JawTransformMapper>());
                KillChildren(Clone.GetComponentsInChildren<ButtPuppet>());
                KillChildren(Clone.GetComponentsInChildren<IKButtPuppet>());
                KillChildren(Clone.GetComponentsInChildren<Blink>());
                KillChildren(Clone.GetComponentsInChildren<HeadPuppet>());
                KillChildren(Clone.GetComponentsInChildren<JawBlendShapeMapper>());
                KillChildren(Clone.GetComponentsInChildren<FaceCapBlendShapeMapper>());
                KillChildren(Clone.GetComponentsInChildren<JointFollower>());
                KillChildren(Clone.GetComponentsInChildren<LookAtTarget>());
                KillChildren(Clone.GetComponentsInChildren<CaptureMicrophone>());
                KillChildren(Clone.GetComponentsInChildren<OneShotAnimations>());
                KillChildren(Clone.GetComponentsInChildren<AudioReference>());
                KillChildren(Clone.GetComponentsInChildren<IKTag>());
                KillChildren(Clone.GetComponentsInChildren<Animator>());
                //There are more components on the clone that are potentially unnessary

                Animator AnimatorTemplate = Clone.AddComponent<Animator>(); 

                AssetDatabase.CopyAsset("Assets/Resources/OneShots.controller", "Assets/Resources/OneShotsTemp.controller");
                AnimatorTemplate.runtimeAnimatorController =  Resources.Load("OneShotsTemp") as RuntimeAnimatorController;

                AnimatorOverrideController AnimatorOverride = new AnimatorOverrideController(AnimatorTemplate.runtimeAnimatorController);
                AnimatorTemplate.runtimeAnimatorController = AnimatorOverride;
                AnimatorOverride["BaseAnimation"] = Performance;
                Clone.AddComponent<OneShotKeybinding>(); 
                AnimationCoroutine = CoroutineHolder.GetComponent<MonoBehaviour>().StartCoroutine(AnimationEndCheck(AnimatorTemplate));
                
                if (Recorder == null)
                    Recorder = EditorWindow.GetWindow<RecorderWindow>();
                
                Recorder.StartRecording();
                TakeCount += 1;
                SetRecorderTarget(Clone);

                //if export window exists
                //if ( (Resources.FindObjectsOfTypeAll(typeof(ExportPerformance)) as ExportPerformance[]).Length > 0 )
                //{
                    EditorWindow.GetWindow<ExportPerformance>().PrefabOverwrite = Actor;
                //}
            }
        }


        [GUIColor(0.9f, 0.3f, 0.3f)]
        [Button(ButtonSizes.Large)]
        [DisableIf("IsAudRef")]
        [HideIf("NotPlaying", false)]
        [ShowIf("IsPlaying", false)]
        [DisableInEditorMode]
        private void Stop()
        {
            
            if (Clone)
            {
                //foreach (Renderer renderer in activeRenderers)
                //    renderer.enabled = true;

                Actor.SetActive(true);

                Recorder.StopRecording();

               if (HubConnectionCheck()){
                    HubConnection.SendSocketMessage("COMMAND;PLAYBACK;STOP;" + ParsedClipNameAfterPlay);
                    HubConnection.SendSocketMessage("COMMAND;PLAYBACK;LOAD;" + ParsedClipName);
               }

                Destroy(Clone);
                Destroy(CoroutineHolder);
                AssetDatabase.DeleteAsset("Assets/Resources/OneShotsTemp.controller");
            }
        }

        private void Update()
        {
            if (EditorApplication.isPlaying)
            {
                if (PlayModeEntered == false)
                {
                    if (!string.IsNullOrEmpty(ParsedClipName) && ParsedClipName.Contains("-")) //create safer check
                    {
                        if (HubConnectionCheck())
                            HubConnection.SendSocketMessage("COMMAND;PLAYBACK;LOAD;" + ParsedClipName);
                    }
                    PlayModeEntered = true;
                    TakeCount = 0;
                }

                //Do you need to gurantee that Recorder exists? 
                if (ModeAccess.Mode == RecorderHelper.AudioModes.AudRef && Recorder.IsRecording())
                {
                    Stop();
                    Repaint();
                }
            }
            else
            {
                if (PlayModeEntered == true)
                    PlayModeEntered = false;                
            }
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

                
                RecordedName = recorder.FileNameGenerator.FileName;
                RecordedName = RecordedName.Replace("<Take>", recorder.Take.ToString("000"));
                Debug.Log("oneshots  " + RecordedName);
                RecordedPaddedName = Performance.name + "." + TakeCount.ToString().PadLeft(3, '0');
                recorder.OutputFile = RecordedPaddedName;

                /*
                recorder.FileNameGenerator.FileName = "";
                recorder.OutputFile = "";
                recorder.Take = 999999;
                */

                foreach (var input in recorder.InputsSettings) { ((AnimationInputSettings)input).gameObject = Clone; }

                return;
            }
        }
        
        public void ParseAnimationClip()
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

                    if (Recorder == null)
                        Recorder = EditorWindow.GetWindow<RecorderWindow>();

                    if (HubConnectionCheck() && EditorApplication.isPlaying && !Recorder.IsRecording())
                        HubConnection.SendSocketMessage("COMMAND;PLAYBACK;LOAD;" + ParsedClipName);
                }
                else
                    ParsedClipName = "Error: Format";
            }
        }

        private bool HubConnectionCheck()
        {   
            //Check to see if name is in the correct format?

            if (HubConnection != FindObjectOfType<MrPuppetHubConnection>() || !HubConnection)
                HubConnection = FindObjectOfType<MrPuppetHubConnection>();

            if (HubConnection)
                return true;

            return false;
        }

        private IEnumerator AnimationEndCheck(Animator animator)
        {
            while ((animator.GetCurrentAnimatorStateInfo(0).normalizedTime) % 1 < 0.99f)
                yield return null;
            
            Stop();
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
