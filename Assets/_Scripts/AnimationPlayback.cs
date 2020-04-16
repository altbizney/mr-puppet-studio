using System.Collections.Generic;
using UnityEngine;
using System.Collections;

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

        /*private new void OnEnable()
        {
            infoBoxMsg = "The audio file has NOT been succesfully loaded yet...";
        }*/

        private GameObject Clone;
        //private AudioClip audioClip;
        //private AudioSource audioSource;
        //private bool inCoroutine;
        private List<Renderer> ActiveRenderers;
        //private string hyperMeshURL;
        private static string TempController = "temp.controller";
        private static MrPuppetHubConnection HubConnection;
        private Coroutine AnimationCoroutine;


        [SerializeField]
        private GameObject Actor;

        [SerializeField]
        [HorizontalGroup(MarginLeft = 0.01f)]
        [AssetSelector(Paths = "Assets/Recordings", FlattenTreeView = true)]
        private AnimationClip _AnimationClip;

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
            if (Clone)
            {
                if (AnimationCoroutine != null)
                    Actor.GetComponent<MonoBehaviour>().StopCoroutine(AnimationCoroutine);

                foreach (Renderer renderer in ActiveRenderers)
                    renderer.enabled = true;

                //audioSource.Stop();
                if (HubConnection != null)
                    HubConnection.SendSocketMessage("COMMAND;PLAYBACK;STOP;" + _AnimationClip.name);

                Destroy(Clone);
                AssetDatabase.DeleteAsset("Assets/Recordings/" + TempController);
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

            if (!Clone)//&& audioClip.loadState == AudioDataLoadState.Loaded
            {

                Clone = Instantiate(Actor, Actor.transform.position, Quaternion.identity);

                KillChildren(Clone.GetComponentsInChildren<JawTransformMapper>());
                KillChildren(Clone.GetComponentsInChildren<ButtPuppet>());
                KillChildren(Clone.GetComponentsInChildren<Blink>());
                KillChildren(Clone.GetComponentsInChildren<HeadPuppet>());
                KillChildren(Clone.GetComponentsInChildren<JawBlendShapeMapper>());
                KillChildren(Clone.GetComponentsInChildren<JointFollower>());
                KillChildren(Clone.GetComponentsInChildren<LookAtTarget>());
                KillChildren(Clone.GetComponentsInChildren<CaptureMicrophone>());
                KillChildren(Clone.GetComponentsInChildren<OneShotAnimations>());

                Animator cloneAnim = Clone.AddComponent<Animator>();
                cloneAnim.enabled = true;
                cloneAnim.runtimeAnimatorController = AnimatorController.CreateAnimatorControllerAtPathWithClip("Assets/Recordings/" + TempController, _AnimationClip);
                AnimationCoroutine = Actor.GetComponent<MonoBehaviour>().StartCoroutine(StopAfterAnimation(cloneAnim));

                foreach (Renderer renderer in Actor.GetComponentsInChildren<Renderer>())
                {
                    if (renderer.enabled == true)
                    {
                        ActiveRenderers.Add(renderer);
                        renderer.enabled = false;
                    }
                }

                //audioSource = Actor.AddComponent<AudioSource>();
                //audioSource.clip = audioClip;
                //audioSource.Play();

                HubConnection = FindObjectOfType<MrPuppetHubConnection>();
                if (HubConnection != null)
                    HubConnection.SendSocketMessage("COMMAND;PLAYBACK;START;" + _AnimationClip.name);
            }
        }

        private void KillChildren(UnityEngine.Object[] children)
        {
            foreach (UnityEngine.Object child in children)
                Destroy(child);
        }

        private bool NotPlaying() { return !Clone; }
        private bool IsPlaying() { return Clone; }

        private void Update()
        {
            if (!EditorApplication.isPlaying)
            {
                if (HubConnection == FindObjectOfType<MrPuppetHubConnection>())
                {
                    if (HubConnection != null)
                        HubConnection.SendSocketMessage("COMMAND;PLAYBACK;STOP;" + _AnimationClip.name);
                    HubConnection = null;
                }
            }
        }

        private IEnumerator StopAfterAnimation(Animator animator)
        {
            while ((animator.GetCurrentAnimatorStateInfo(0).normalizedTime) % 1 < 0.99f)
                yield return null;
            StopAnimation();
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
                AssetDatabase.DeleteAsset("Assets/Recordings/" + TempController);
            }
        }
    }
#else
public class AnimationPlayback : MonoBehaviour {

}
#endif
}
