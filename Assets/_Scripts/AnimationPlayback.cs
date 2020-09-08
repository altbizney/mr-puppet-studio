using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
#endif

namespace MrPuppet
{
#if UNITY_EDITOR
    public class AnimationPlayback : OdinEditorWindow
    {
        [MenuItem("Tools/Animation Playback")]
        private static void OpenWindow()
        {
            GetWindow<AnimationPlayback>().Show();
        }

        private new void OnEnable()
        {
            //infoBoxMsg = "The audio file has NOT been succesfully loaded yet...";
            ParseMessageBoxUpdate();
        }

        private GameObject clone;
        //private AudioClip audioClip;
        //private AudioSource audioSource;
        //private bool inCoroutine;
        private List<Renderer> activeRenderers;
        //private string hyperMeshURL;
        private static string tempController = "temp.controller";
        private static MrPuppetHubConnection HubConnection;
        private Coroutine AnimationCoroutine;
        private string ParseMessageBox;
        private string AudioClipParseAfterPlay = "Waiting for Animation Clip";
        private bool PlayModeEntered;

        [SerializeField]
        [InfoBox("$ParseMessageBox")]
        private GameObject Actor;

        [SerializeField]
        [HorizontalGroup(MarginLeft = 0.01f)]
        [AssetSelector(Paths = "Assets/Recordings", FlattenTreeView = true)]
        [OnValueChanged("ParseMessageBoxUpdate")]
        private AnimationClip _AnimationClip;

        [OnValueChanged("ParseMessageBoxUpdate")]
        [LabelText("Use full audio take name")]
        [ToggleLeft]
        public bool EnableAudioParse;

        /*[SerializeField]
        [BoxGroup]
        [DisplayAsString]
        [HideLabel]
        private string infoBoxMsg;*/

        /*
        [Button("Latest")]
        [HorizontalGroup(Width = 35)]
        private void LatestAnimation()
        {
            List<string> files = Directory.GetFiles("Assets/Recordings").OrderBy(f => f).ToList();
            animationClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(files[files.Count - 2], typeof(AnimationClip));
        }
        */
        [GUIColor(0.9f, 0.3f, 0.3f)]
        [Button(ButtonSizes.Large)]
        [HideIf("NotPlaying", false)]
        [ShowIf("IsPlaying", false)]
        [DisableInEditorMode]
        private void StopAnimation()
        {
            if (clone)
            {
                if (AnimationCoroutine != null)
                    Actor.GetComponent<MonoBehaviour>().StopCoroutine(AnimationCoroutine);

                foreach (Renderer renderer in activeRenderers)
                    renderer.enabled = true;

                if (HubConnectionCheck())
                    HubConnection.SendSocketMessage("COMMAND;PLAYBACK;STOP;" + AudioClipParseAfterPlay);

                Destroy(clone);
                AssetDatabase.DeleteAsset("Assets/Recordings/" + tempController);
            }
        }

        [GUIColor(0.2f, 0.9f, 0.2f)]
        [Button(ButtonSizes.Large)]
        [HideIf("IsPlaying", false)]
        [ShowIf("NotPlaying", false)]
        [DisableInEditorMode]
        private void PlayAnimation()
        {
            //if (!audioClip)//&& !inCoroutine
            //Actor.GetComponent<MonoBehaviour>().StartCoroutine(QueryHyperMesh("https://hypermesh.app/performances/" + _AnimationClip.name + "-audio/info.json"));
            //else
            InitializeAnimation();
        }

        private void InitializeAnimation()
        {

            if (!clone) //&& audioClip.loadState == AudioDataLoadState.Loaded
            {

                clone = Instantiate(Actor, Actor.transform.position, Quaternion.identity);

                KillChildren(clone.GetComponentsInChildren<JawTransformMapper>());
                KillChildren(clone.GetComponentsInChildren<ButtPuppet>());
                KillChildren(clone.GetComponentsInChildren<Blink>());
                KillChildren(clone.GetComponentsInChildren<HeadPuppet>());
                KillChildren(clone.GetComponentsInChildren<JawBlendShapeMapper>());
                KillChildren(clone.GetComponentsInChildren<JointFollower>());
                KillChildren(clone.GetComponentsInChildren<LookAtTarget>());
                KillChildren(clone.GetComponentsInChildren<CaptureMicrophone>());
                KillChildren(clone.GetComponentsInChildren<OneShotAnimations>());
                KillChildren(clone.GetComponentsInChildren<IKButtPuppet>());

                Animator cloneAnim = clone.AddComponent<Animator>();
                cloneAnim.enabled = true;
                cloneAnim.runtimeAnimatorController = AnimatorController.CreateAnimatorControllerAtPathWithClip("Assets/Recordings/" + tempController, _AnimationClip);
                AnimationCoroutine = Actor.GetComponent<MonoBehaviour>().StartCoroutine(StopAfterAnimation(cloneAnim));

                foreach (Renderer renderer in Actor.GetComponentsInChildren<Renderer>())
                {
                    if (renderer.enabled == true)
                    {
                        activeRenderers.Add(renderer);
                        renderer.enabled = false;
                    }
                }

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
                //audioSource = Actor.AddComponent<AudioSource>();
                //audioSource.clip = audioClip;
                //audioSource.Play();
            }
        }

        private void Update()
        {
            if (!EditorApplication.isPlaying)
            {
                if ((HubConnection == FindObjectOfType<MrPuppetHubConnection>() || HubConnection) && !string.IsNullOrEmpty(AudioClipParseAfterPlay) && AudioClipParseAfterPlay != "Waiting for Animation Clip")
                {
                    HubConnection.SendSocketMessage("COMMAND;PLAYBACK;STOP;" + AudioClipParseAfterPlay);
                    HubConnection = null;
                    AudioClipParseAfterPlay = "";
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

        private void KillChildren(UnityEngine.Object[] children)
        {
            foreach (UnityEngine.Object child in children)
                Destroy(child);
        }

        private IEnumerator StopAfterAnimation(Animator animator)
        {
            while ((animator.GetCurrentAnimatorStateInfo(0).normalizedTime) % 1 < 0.99f)
                yield return null;
            StopAnimation();
        }

        private bool NotPlaying()
        {
            return !clone;
        }

        private bool IsPlaying()
        {
            return clone;
        }

        /*
        private IEnumerator QueryHyperMesh(string url)
        {
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
                        Actor.GetComponent<MonoBehaviour>().StartCoroutine(StreamAudioClip(json.media_url));
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
        */

        /*
        private IEnumerator StreamAudioClip(string url)
        {
            inCoroutine = true;
            using (UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
            {
                infoBoxMsg = "LOADING AUDIO FILE - 0%";
                webRequest.SendWebRequest();
                while (!webRequest.isDone)
                {
                    infoBoxMsg = "LOADING AUDIO FILE - " + Mathf.Round((webRequest.downloadProgress * 100)) + "%";
                    yield return null;
                }
                if (webRequest.isNetworkError)
                {
                    Debug.Log("Error");
                    Debug.Log(webRequest.error);
                }
                else
                {
                    audioClip = DownloadHandlerAudioClip.GetContent(webRequest);
                    inCoroutine = false;
                    infoBoxMsg = "The audio file has been succesfully loaded!";
                    InitializeAnimation();
                }
            }
        }
        */

        /*
        //Public variables should currently only be visible to enclosing type.
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

        [InitializeOnLoadAttribute]
        static class PlayModeStateChanged
        {
            static PlayModeStateChanged()
            {
                EditorApplication.playModeStateChanged += playModes;
            }

            private static void playModes(PlayModeStateChange state)
            {
                AssetDatabase.DeleteAsset("Assets/Recordings/" + tempController);
            }
        }
    }
#else
    public class AnimationPlayback : MonoBehaviour
    {

    }
#endif
}
