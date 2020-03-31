using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEditor.Recorder.Timeline;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Formats.Fbx.Exporter;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor.Recorder;
#endif


namespace MrPuppet
{
#if UNITY_EDITOR

    public class PerformanceAnimationReplacementManager : OdinEditorWindow
    {

        [MenuItem("Tools/Performance Animation Replacement")]
        private static void OpenWindow()
        {
            GetWindow<PerformanceAnimationReplacementManager>().Show();
        }

        private new void OnEnable()
        {
            InfoBoxMsg = "The audio file has NOT been succesfully loaded yet...";
        }

        private GameObject PuppetReplay;
        private AudioClip _AudioClip;
        private AudioSource _AudioSource;
        private Transform JawJointMimic;
        private bool InCoroutine;
        private GameObject TransformWrapper;
        private RecorderWindow Recorder;
        private Transform JawJoint;
        private Coroutine AnimationCoroutine;
        //private AnimationEvent _aEvents;
        //private AnimationEvent _aEvents2;

        private bool OverwriteButtPuppet;
        private bool OverwriteJaw;

        public GameObject Actor;
        public AnimationClip _AnimationClip;

        [SerializeField]
        [BoxGroup]
        [DisplayAsString]
        [HideLabel]
        private string InfoBoxMsg;

        // TODO: Propperties are probably a better solution to how to detect when a bool changes
        /*public bool _OverwriteButtPuppet
        {
            get {return OverwriteButtPuppet;}

            set
            {
                if (OverwriteButtPuppet != value)
                {
                    OverwriteButtPuppet = value;
                    Debug.Log("CHNAGED");
                }
            }
        }

        public bool _OverwriteJaw
        {
            get { return OverwriteJaw; }

            set
            {
                if (OverwriteJaw != value)
                {
                    OverwriteJaw = value;
                    Debug.Log("CHANGED");
                }
            }
        }*/

        private void KillChildren(UnityEngine.Object[] children)
        {
            foreach (UnityEngine.Object child in children)
                Destroy(child);
        }

        private bool DisableRecordOption()
        {
            if (_AnimationClip == null || Actor == null)
                return true;
            else
                return false;
        }

        private bool NotPlaying()
        {
            return !PuppetReplay;
        }

        private bool IsPlaying()
        {
            return PuppetReplay;
        }

        [GUIColor(0.2f, 0.9f, 0.2f)]
        [Button(ButtonSizes.Large)]
        [ButtonGroup]
        [DisableIf("DisableRecordOption")]
        [HideIf("IsPlaying", false)]
        [ShowIf("NotPlaying", false)]
        [DisableInEditorMode]
        private void RerecordJaw(){
            if (!PuppetReplay && !InCoroutine)
                Actor.GetComponent<MonoBehaviour>().StartCoroutine(QueryHyperMesh("https://hypermesh.app/performances/" + _AnimationClip.name + "-audio/info.json"));

            OverwriteButtPuppet = true;
            OverwriteJaw = false;
        }

        [GUIColor(0.2f, 0.9f, 0.2f)]
        [Button(ButtonSizes.Large)]
        [ButtonGroup]
        [DisableIf("DisableRecordOption")]
        [HideIf("IsPlaying", false)]
        [ShowIf("NotPlaying", false)]
        [DisableInEditorMode]
        private void RerecordButtPuppet()
        {
            if (!PuppetReplay && !InCoroutine)
                Actor.GetComponent<MonoBehaviour>().StartCoroutine(QueryHyperMesh("https://hypermesh.app/performances/" + _AnimationClip.name + "-audio/info.json"));

            OverwriteButtPuppet = false;
            OverwriteJaw = true;
        }

        [GUIColor(0.9f, 0.3f, 0.3f)]
        [Button(ButtonSizes.Large)]
        [HideIf("NotPlaying", false)]
        [ShowIf("IsPlaying", false)]
        [DisableInEditorMode]
        private void StopAnimation()
        {
            if (PuppetReplay)
            {
               if (AnimationCoroutine != null)
                 Actor.GetComponent<MonoBehaviour>().StopCoroutine(AnimationCoroutine);

                //_AudioSource.Stop();
                TransformWrapper.transform.DetachChildren();

                Destroy(TransformWrapper);
                Destroy(PuppetReplay);
                //Destroy(Recorder);

                InfoBoxMsg = "The audio file has NOT been succesfully loaded yet...";

                if (!Actor.GetComponent<ButtPuppet>().ApplySensors)
                    Actor.GetComponent<ButtPuppet>().ApplySensors = true;

                if (!Actor.GetComponent<JawTransformMapper>().ApplySensors)
                    Actor.GetComponent<JawTransformMapper>().ApplySensors = true;

                AssetDatabase.DeleteAsset("Assets/Recordings/tempPAR.controller");
            }
        }

        /*
        [Button(ButtonSizes.Large)]
        [DisableInEditorMode]
        private void Test()
        {

            GameObject TestObject = new GameObject("Test");
            GameObject PuppetTest = Instantiate(Actor, new Vector3(0, 0, -2), Actor.transform.rotation);
            TransformWrapper = new GameObject("TransformWrapper");

            KillChildren(PuppetTest.GetComponentsInChildren<MrPuppet.JawTransformMapper>());
            KillChildren(PuppetTest.GetComponentsInChildren<MrPuppet.ButtPuppet>());
            KillChildren(PuppetTest.GetComponentsInChildren<MrPuppet.Blink>());
            KillChildren(PuppetTest.GetComponentsInChildren<MrPuppet.HeadPuppet>());
            KillChildren(PuppetTest.GetComponentsInChildren<MrPuppet.JawBlendShapeMapper>());
            KillChildren(PuppetTest.GetComponentsInChildren<MrPuppet.JointFollower>());
            KillChildren(PuppetTest.GetComponentsInChildren<MrPuppet.LookAtTarget>());
            KillChildren(PuppetTest.GetComponentsInChildren<MrPuppet.CaptureMicrophone>());
            KillChildren(PuppetTest.GetComponentsInChildren<OneShotAnimations>());

            Animator _Animator = PuppetTest.AddComponent<Animator>();
            //_Animator.runtimeAnimatorController = AnimatorController.CreateAnimatorControllerAtPathWithClip("Assets/Recordings/tempPAR.controller", _AnimationClip);
            //_Animator.updateMode = AnimatorUpdateMode.UnscaledTime;

            PuppetTest.transform.parent = TransformWrapper.transform;
            TransformWrapper.transform.position += new Vector3(0, 0, 2);

            PlayableDirector _PlayableDirector = TestObject.AddComponent<PlayableDirector>();
            //TimelineAsset _TimelineAsset = new TimelineAsset();
            _PlayableDirector.playableAsset = _TimelineAsset;
            var NewTrack = _TimelineAsset.CreateTrack<AnimationTrack>(null, "Test Track");
            NewTrack.CreateClip(_AnimationClip);//tell track what animation clip
            _PlayableDirector.SetGenericBinding(NewTrack, PuppetTest);//set animator for newtrack
            _PlayableDirector.Play();


            //NewTrack.CreateClip<AnimationClip>();
            //RecorderClip sso = new RecorderClip();
            //sso.CreatePlayable
            //var RecTrack = _TimelineAsset.CreateTrack<RecorderTrack>(null, "Test Record");
            //RecTrack.CreateDefaultClip();
        }
        */

        private void InitializeAnimation()
        {
            JawJoint = Actor.GetComponent<JawTransformMapper>().JawJoint;

            PuppetReplay = Instantiate(Actor, new Vector3(0, 0, 0), Actor.transform.rotation);

            KillChildren(PuppetReplay.GetComponentsInChildren<MrPuppet.JawTransformMapper>());
            KillChildren(PuppetReplay.GetComponentsInChildren<MrPuppet.ButtPuppet>());
            KillChildren(PuppetReplay.GetComponentsInChildren<MrPuppet.Blink>());
            KillChildren(PuppetReplay.GetComponentsInChildren<MrPuppet.HeadPuppet>());
            KillChildren(PuppetReplay.GetComponentsInChildren<MrPuppet.JawBlendShapeMapper>());
            KillChildren(PuppetReplay.GetComponentsInChildren<MrPuppet.JointFollower>());
            KillChildren(PuppetReplay.GetComponentsInChildren<MrPuppet.LookAtTarget>());
            KillChildren(PuppetReplay.GetComponentsInChildren<MrPuppet.CaptureMicrophone>());
            KillChildren(PuppetReplay.GetComponentsInChildren<OneShotAnimations>());
            KillChildren(PuppetReplay.GetComponentsInChildren<Animator>());

            //_AudioSource = PuppetReplay.AddComponent<AudioSource>();
            //_AudioSource.clip = _AudioClip;
            //_AudioSource.Play();

            JointController _JointController = PuppetReplay.AddComponent<JointController>();
            _JointController.PAR = this;

            Animator cloneReplay = PuppetReplay.AddComponent<Animator>();
            cloneReplay.runtimeAnimatorController = AnimatorController.CreateAnimatorControllerAtPathWithClip("Assets/Recordings/tempPAR.controller", _AnimationClip);
            //cloneReplay.updateMode = AnimatorUpdateMode.UnscaledTime;
            //_aEvents = new AnimationEvent[1];
            //_aEvents = new AnimationEvent();
            //_aEvents.functionName = "StartRecord";
            //_aEvents.time = 0f;
            //_aEvents2 = new AnimationEvent();
            //_aEvents2.functionName = "StopRecord";
            // _aEvents2.time = 18;//cloneReplay.GetCurrentAnimatorClipInfo(0).Length;
            //_AnimationClip.AddEvent(_aEvents);
            //_AnimationClip.AddEvent(_aEvents2);
            //Debug.Log(_AnimationClip.length);
            //Debug.Log((float)System.Math.Round(_AnimationClip.length, 3));
            AnimationCoroutine = Actor.GetComponent<MonoBehaviour>().StartCoroutine( StopAfterAnimation() );

            TransformWrapper = new GameObject("TransformWrapper");
            PuppetReplay.transform.parent = TransformWrapper.transform;
            TransformWrapper.transform.position += new Vector3(0, 0, 2);
            Actor.transform.parent = TransformWrapper.transform;

            foreach (Transform child in PuppetReplay.transform.GetComponentsInChildren<Transform>())
            {
                if (child.name == JawJoint.name)
                {
                    JawJointMimic = child;
                }
            }

            if (!Recorder)
                Recorder = EditorWindow.GetWindow<RecorderWindow>();
            Recorder.StartRecording();
        }


        private IEnumerator StopAfterAnimation()
        {
            yield return new WaitForSeconds((float) System.Math.Round(_AnimationClip.length, 3) );

            if (Recorder)
            {
              if(Recorder.IsRecording())
                Recorder.StopRecording();
            }

            StopAnimation();
        }   

        private IEnumerator QueryHyperMesh(string url)
        {
            InCoroutine = true;

            JsonData json;

            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            webRequest.SetRequestHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJVc2VyOlZhbmhOSiJ9.3FXNh0U-DOJ76GwtFQJx1wblRzVFAPJElIrtN15pQEM");
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                try
                {
                    json = JsonData.CreateFromJSON(webRequest.downloadHandler.text);

                    if (json.ok == true)
                    {
                        Actor.GetComponent<MonoBehaviour>().StartCoroutine(Stream_AutioClip(json.media_url));
                    }
                    else
                    {
                        if (json.error != null)
                            Debug.Log("ERROR : " + json.error);
                        else
                            Debug.Log("ERROR : UNKNOWN");
                    }
                }
                catch
                {
                    Debug.Log("ERROR: UNKNOWN");
                }
            }
        }

        private IEnumerator Stream_AutioClip(string url)
        {
            InCoroutine = true;
            using (UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
            {
                InfoBoxMsg = "LOADING AUDIO FILE";
                webRequest.SendWebRequest();

                while (!webRequest.isDone)
                {
                    InfoBoxMsg = "LOADING AUDIO FILE";
                    yield return null;
                }

                if (webRequest.isNetworkError)
                {
                    Debug.Log("Error");
                    Debug.Log(webRequest.error);
                }
                else
                {
                    _AudioClip = DownloadHandlerAudioClip.GetContent(webRequest);
                    InCoroutine = false;
                    InfoBoxMsg = "The audio file has been succesfully loaded!";
                    InitializeAnimation();
                }
            }
        }

        private class JsonData
        {
            public bool ok;
            public string name;
            public string media_status;
            public string media_url;

            public string status;
            public string error;

            public static JsonData CreateFromJSON(string jsonString)
            {
                return JsonUtility.FromJson<JsonData>(jsonString);
            }
        }

        public class JointController : MonoBehaviour
        {
            public PerformanceAnimationReplacementManager PAR;

            void LateUpdate()
            {
                if (PAR.JawJointMimic != null && PAR.PuppetReplay)
                {
                    if (PAR.OverwriteJaw == true)
                    {
                        if (PAR.Actor.GetComponent<JawTransformMapper>().ApplySensors)
                            PAR.Actor.GetComponent<JawTransformMapper>().ApplySensors = false;

                        PAR.JawJoint.localRotation = PAR.JawJointMimic.localRotation;
                        PAR.JawJoint.localPosition = PAR.JawJointMimic.localPosition;

                    }
                    else
                    {
                        if (!PAR.Actor.GetComponent<JawTransformMapper>().ApplySensors)
                            PAR.Actor.GetComponent<JawTransformMapper>().ApplySensors = true;

                    }

                    if (PAR.OverwriteButtPuppet == true)
                    {
                        if (PAR.Actor.GetComponent<ButtPuppet>().ApplySensors)
                            PAR.Actor.GetComponent<ButtPuppet>().ApplySensors = false;

                        //Nasty loop. TODO: More performant solution. 
                        foreach (Transform child in PAR.PuppetReplay.transform.GetComponentsInChildren<Transform>())
                        {
                            if (child.name != PAR.JawJointMimic.name)
                            {
                                foreach (Transform nestedChild in PAR.Actor.transform.GetComponentsInChildren<Transform>())
                                {
                                    if (nestedChild.name == child.name)
                                    {
                                        nestedChild.localRotation = child.localRotation;
                                        nestedChild.localPosition = child.localPosition;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!PAR.Actor.GetComponent<ButtPuppet>().ApplySensors)
                            PAR.Actor.GetComponent<ButtPuppet>().ApplySensors = true;

                    }
                }
            }

            /*
            public void StartRecord()
            {
                Debug.Log("Working");
                if (!PAR.Recorder)
                    PAR.Recorder = EditorWindow.GetWindow<RecorderWindow>();
                PAR.Recorder.StartRecording();
            }

            public void StopRecord()
            {
                Debug.Log("Stopping Audio");

                //if (PAR.AnimationCoroutine != null)
                  //PAR.Actor.GetComponent<MonoBehaviour>().StopCoroutine(PAR.AnimationCoroutine);

                //PAR.TransformWrapper.transform.DetachChildren();

                //Destroy(PAR.TransformWrapper);
               // Destroy(PAR.PuppetReplay);
               // Destroy(PAR.Recorder);

                if (PAR.Recorder)
                {
                    if (PAR.Recorder.IsRecording())
                    {
                        Debug.Log("Stopping vid 2");

                        PAR.Recorder.StopRecording();
                    }
                }
            }*/
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
               if(state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                AssetDatabase.DeleteAsset("Assets/Recordings/tempPAR.controller");
            }
        }
    }
#else
public class PerformanceAnimationReplacementManager : MonoBehaviour {

}
#endif
}
