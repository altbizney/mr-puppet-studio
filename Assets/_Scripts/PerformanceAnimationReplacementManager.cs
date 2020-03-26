using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Formats.Fbx.Exporter;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
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

        public GameObject Actor;
        public Transform JawJoint;//TODO: Get automatically from JawMapper?
        public AnimationClip _AnimationClip;

        [SerializeField]
        [BoxGroup]
        [DisplayAsString]
        [HideLabel]
        private string InfoBoxMsg;

        [HorizontalGroup]
        public bool OverwriteButtPuppet;
        [HorizontalGroup]
        public bool OverwriteJaw;


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

        public class JointController : MonoBehaviour
        {
            public PerformanceAnimationReplacementManager PAR;

            void LateUpdate()
                {
                        if (PAR.JawJointMimic != null && PAR.PuppetReplay )
                        {
                            if (PAR.OverwriteJaw == true)
                            {
                                if (PAR.Actor.GetComponent<JawTransformMapper>().ApplySensors)
                                    PAR.Actor.GetComponent<JawTransformMapper>().ApplySensors = false;

                               PAR.JawJoint.localRotation = PAR.JawJointMimic.localRotation;
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
        }

        [GUIColor(0.2f, 0.9f, 0.2f)]
        [Button(ButtonSizes.Large)]
        [DisableInEditorMode]
        private void Play(){
            if (!PuppetReplay && !InCoroutine)
                Actor.GetComponent<MonoBehaviour>().StartCoroutine(QueryHyperMesh("https://hypermesh.app/performances/" + _AnimationClip.name + "-audio/info.json"));
        }

        private void InitializeAnimation()
        {
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

            _AudioSource = PuppetReplay.AddComponent<AudioSource>();
            _AudioSource.clip = _AudioClip;
            _AudioSource.Play();

            JointController _JointController = PuppetReplay.AddComponent<JointController>();
            _JointController.PAR = this;

            Animator cloneReplay = PuppetReplay.AddComponent<Animator>();
            cloneReplay.runtimeAnimatorController = AnimatorController.CreateAnimatorControllerAtPathWithClip("Assets/Recordings/tempPAR.controller", _AnimationClip);
            cloneReplay.updateMode = AnimatorUpdateMode.UnscaledTime;
            Actor.GetComponent<MonoBehaviour>().StartCoroutine(StopAfterAnimation(cloneReplay));
            Actor.transform.position = PuppetReplay.transform.position + new Vector3(0, 0, 1.5f);


            foreach (Transform child in PuppetReplay.transform.GetComponentsInChildren<Transform>())
            {
                if (child.name == JawJoint.name)
                {
                    JawJointMimic = child;
                }
            }
        }

        private void KillChildren(UnityEngine.Object[] children)
        {
            foreach (UnityEngine.Object child in children)
                Destroy(child);
        }

        private void StopAnimation()
        {
            if (PuppetReplay)
            {
                _AudioSource.Stop();
                Destroy(PuppetReplay);
                AssetDatabase.DeleteAsset("Assets/Recordings/tempPAR.controller");

                InfoBoxMsg = "The audio file has NOT been succesfully loaded yet...";

                if (!Actor.GetComponent<ButtPuppet>().ApplySensors)
                    Actor.GetComponent<ButtPuppet>().ApplySensors = true;

                if (!Actor.GetComponent<JawTransformMapper>().ApplySensors)
                    Actor.GetComponent<JawTransformMapper>().ApplySensors = true;
            }
        }

        private IEnumerator StopAfterAnimation(Animator animator)
        {
            while ((animator.GetCurrentAnimatorStateInfo(0).normalizedTime) % 1 < 0.99f)
                yield return null;
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

        [InitializeOnLoadAttribute]
        static class PlayModeStateChanged
        {
            static PlayModeStateChanged()
            {
                EditorApplication.playModeStateChanged += playModes;
            }

            private static void playModes(PlayModeStateChange state)
            {
                AssetDatabase.DeleteAsset("Assets/Recordings/tempPAR.controller");
            }
        }
    }
#else
public class PerformanceAnimationReplacementManager : MonoBehaviour {

}
#endif
}
