﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
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
            //InfoBoxMsg = "The audio file has NOT been succesfully loaded yet...";
        }

        private GameObject PuppetReplay;
        //private AudioClip _AudioClip;
        //private AudioSource _AudioSource;
        private Transform JawJointMimic;
        //private bool InCoroutine;
        private GameObject TransformWrapper;
        private RecorderWindow Recorder;
        private Transform JawJoint;
        private Coroutine AnimationCoroutine;
        private static MrPuppetHubConnection HubConnection;
        //private static string _AnimationClipName;

        private List<Transform> JointsMimic;
        private List<Transform> JointsClone;

        private bool OverwriteButtPuppet;
        private bool OverwriteJaw;

        //[OnValueChanged("InitComponents")]
        [InfoBox("$ParseMessageBox")]
        public GameObject Actor;

        [OnValueChanged("ParseMessageBoxUpdate")]
        public AnimationClip _AnimationClip;

        //private string ButtComponent;
        private string ParseMessageBox;
        private string AudioClipParseAfterPlay = "Waiting for Animation Clip";
        private bool PlayModeEntered;

        [OnValueChanged("ParseMessageBoxUpdate")]
        [LabelText("Use full audio take name")]
        [ToggleLeft]
        public bool EnableAudioParse;

        /*
        [SerializeField]
        [BoxGroup]
        [DisplayAsString]
        [HideLabel]
        private string InfoBoxMsg;
        */

        /*
        private void InitComponents()
        {
            if ((Actor.GetComponent("ButtPuppet") as ButtPuppet) != null)
                ButtComponent = "ButtPuppet";
            else if ((Actor.GetComponent("IKButtPuppet") as IKButtPuppet) != null)
                ButtComponent = "IKButtPuppet";

        }
        */
        private bool HubConnectionCheck()
        {
            if (HubConnection != FindObjectOfType<MrPuppetHubConnection>() || !HubConnection)
                HubConnection = FindObjectOfType<MrPuppetHubConnection>();

            if (HubConnection)
                return true;

            return false;
        }

        private void ParseMessageBoxUpdate()
        {
            if (_AnimationClip)
            {
                if (_AnimationClip.name.Count(c => c == '-') == 2)
                {
                    if (!EnableAudioParse)
                    {
                        ParseMessageBox = _AnimationClip.name.Substring(0, _AnimationClip.name.LastIndexOf("-"));
                    }
                    else
                    {
                        ParseMessageBox = _AnimationClip.name;
                    }

                    if (HubConnectionCheck() && EditorApplication.isPlaying)
                        HubConnection.SendSocketMessage("COMMAND;PLAYBACK;LOAD;" + ParseMessageBox);
                }
                else
                    ParseMessageBox = "Please input Animation Clip with correct format";
            }
            else
                ParseMessageBox = "Waiting for Animation Clip";
        }

        private void SetButtPuppetSensors(bool SensorBoolean)
        {
            if (Actor)
            {
                if (Actor.GetComponent<ButtPuppet>() != null)
                {
                    if (SensorBoolean == false)
                    {
                        if (Actor.GetComponent<ButtPuppet>().ApplySensors)
                            Actor.GetComponent<ButtPuppet>().ApplySensors = false;
                    }
                    else
                    {
                        if (!Actor.GetComponent<ButtPuppet>().ApplySensors)
                            Actor.GetComponent<ButtPuppet>().ApplySensors = true;
                    }
                }
                else if (Actor.GetComponent<IKButtPuppet>() != null)
                {
                    if (SensorBoolean == false)
                    {
                        if (Actor.GetComponent<IKButtPuppet>().ApplySensors)
                            Actor.GetComponent<IKButtPuppet>().ApplySensors = false;
                    }
                    else
                    {
                        if (!Actor.GetComponent<IKButtPuppet>().ApplySensors)
                            Actor.GetComponent<IKButtPuppet>().ApplySensors = true;
                    }
                }
            }
        }

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
        private void RerecordJaw()
        {

            if (!PuppetReplay)
                InitializeAnimation();
            //Actor.GetComponent<MonoBehaviour>().StartCoroutine(QueryHyperMesh("https://hypermesh.app/performances/" + _AnimationClip.name + "-audio/info.json"));

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
            if (!PuppetReplay)
                InitializeAnimation();
            //Actor.GetComponent<MonoBehaviour>().StartCoroutine(QueryHyperMesh("https://hypermesh.app/performances/" + _AnimationClip.name + "-audio/info.json"));

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
                if (HubConnectionCheck())
                    HubConnection.SendSocketMessage("COMMAND;PLAYBACK;STOP;" + AudioClipParseAfterPlay);

                TransformWrapper.transform.DetachChildren();

                Destroy(TransformWrapper);
                Destroy(PuppetReplay);

                //InfoBoxMsg = "The audio file has NOT been succesfully loaded yet...";

                SetButtPuppetSensors(true);

                if (!Actor.GetComponent<JawTransformMapper>().ApplySensors)
                    Actor.GetComponent<JawTransformMapper>().ApplySensors = true;

                AssetDatabase.DeleteAsset("Assets/Recordings/tempPAR.controller");
            }
        }

        private void InitializeAnimation()
        {
            JawJoint = Actor.GetComponent<JawTransformMapper>().JawJoint;

            PuppetReplay = Instantiate(Actor, new Vector3(0, 0, 0), Actor.transform.rotation);

            KillChildren(PuppetReplay.GetComponentsInChildren<MrPuppet.JawTransformMapper>());
            KillChildren(PuppetReplay.GetComponentsInChildren<MrPuppet.ButtPuppet>());
            KillChildren(PuppetReplay.GetComponentsInChildren<MrPuppet.IKButtPuppet>());
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

            //_AnimationClipName = _AnimationClip.name;
            if (_AnimationClip)
            {
                if (_AnimationClip.name.Count(c => c == '-') == 2)
                {
                    if (!EnableAudioParse)
                        AudioClipParseAfterPlay = _AnimationClip.name.Substring(0, _AnimationClip.name.LastIndexOf("-"));
                    else
                        AudioClipParseAfterPlay = _AnimationClip.name;

                    if (HubConnectionCheck())
                        HubConnection.SendSocketMessage("COMMAND;PLAYBACK;START;" + AudioClipParseAfterPlay);
                }
            }

            JointController _JointController = PuppetReplay.AddComponent<JointController>();
            _JointController.PAR = this;

            Animator cloneReplay = PuppetReplay.AddComponent<Animator>();
            cloneReplay.runtimeAnimatorController = AnimatorController.CreateAnimatorControllerAtPathWithClip("Assets/Recordings/tempPAR.controller", _AnimationClip);
            AnimationCoroutine = Actor.GetComponent<MonoBehaviour>().StartCoroutine(StopAfterAnimation());

            TransformWrapper = new GameObject("TransformWrapper");
            PuppetReplay.transform.parent = TransformWrapper.transform;
            TransformWrapper.transform.position += new Vector3(0, 0, 3.5f);
            Actor.transform.parent = TransformWrapper.transform;

            foreach (Transform child in PuppetReplay.transform.GetComponentsInChildren<Transform>())
            {
                if (child.name == JawJoint.name)
                {
                    JawJointMimic = child;
                }
            }

            JointsClone.Clear();
            JointsMimic.Clear();
            foreach (Transform child in PuppetReplay.transform.GetComponentsInChildren<Transform>())
            {
                if (child.name != JawJointMimic.name)
                {
                    foreach (Transform nestedChild in Actor.transform.GetComponentsInChildren<Transform>())
                    {
                        if (nestedChild.name == child.name)
                        {
                            JointsMimic.Add(nestedChild);
                            JointsClone.Add(child);
                        }
                    }
                }
            }

            if (!Recorder)
                Recorder = EditorWindow.GetWindow<RecorderWindow>();
            Recorder.StartRecording();
        }

        private void Update()
        {
            if (!EditorApplication.isPlaying)
            {
                if (HubConnection == FindObjectOfType<MrPuppetHubConnection>())
                {
                    HubConnection.SendSocketMessage("COMMAND;PLAYBACK;STOP;" + AudioClipParseAfterPlay);
                    HubConnection = null;
                }
                if (PlayModeEntered == true)
                    PlayModeEntered = false;
            }
            else
            {
                if (PlayModeEntered == false)
                {
                    if (_AnimationClip)
                    {
                        if (_AnimationClip.name.Count(c => c == '-') == 2)
                        {
                            if (HubConnectionCheck())
                            {
                                HubConnection.SendSocketMessage("COMMAND;PLAYBACK;LOAD;" + ParseMessageBox);
                            }
                        }
                    }
                    PlayModeEntered = true;
                }
            }
        }

        private IEnumerator StopAfterAnimation()
        {
            yield return new WaitForSeconds((float)System.Math.Ceiling(_AnimationClip.length * 1000) / 1000);

            if (Recorder)
            {
                if (Recorder.IsRecording())
                    Recorder.StopRecording();
            }

            StopAnimation();
        }

        /*
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
         Actor.GetComponent<MonoBehaviour>().StartCoroutine(Stream_AutioClip());
         }
         */

        /*
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
        */

        /*
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
        */

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
                        PAR.SetButtPuppetSensors(false);

                        for (var i = 0; i < PAR.JointsMimic.Count; i++)
                        {
                            PAR.JointsMimic[i].localRotation = PAR.JointsClone[i].localRotation;
                        }

                    }
                    else
                    {
                        PAR.SetButtPuppetSensors(true);
                    }
                }
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
                if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                    AssetDatabase.DeleteAsset("Assets/Recordings/tempPAR.controller");
            }
        }
    }
#else
public class PerformanceAnimationReplacementManager : MonoBehaviour {

}
#endif
}
