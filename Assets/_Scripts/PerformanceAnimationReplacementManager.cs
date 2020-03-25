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

        private GameObject PuppetReplay;
        private AudioClip audioClip;
        private AudioSource audioSource;
        private GameObject JawJointMimic;
        private bool inCoroutine;

        public GameObject actor;
        public Transform JawJoint;//TODO: Get automatically from JawMapper?
        public AnimationClip animationClip;

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

        [GUIColor(0.2f, 0.9f, 0.2f)]
        [Button(ButtonSizes.Large)]
        [DisableInEditorMode]
        private void Play(){
            if (!audioClip && !inCoroutine) // && !inCoroutine
                actor.GetComponent<MonoBehaviour>().StartCoroutine(QueryHyperMesh("https://hypermesh.app/performances/" + animationClip.name + "-audio/info.json"));
        }

        void Update()
        {
            if (EditorApplication.isPlaying && !EditorApplication.isPaused)
            {
                if (JawJointMimic != null)
                {
                    if (OverwriteJaw == true)
                    {
                        if (actor.GetComponent<JawTransformMapper>().ApplySensors)
                            actor.GetComponent<JawTransformMapper>().ApplySensors = false;

                        JawJoint.localRotation = JawJointMimic.transform.localRotation;
                    }
                    else
                    {
                        if (!actor.GetComponent<JawTransformMapper>().ApplySensors)
                            actor.GetComponent<JawTransformMapper>().ApplySensors = true;

                    }

                    if (OverwriteButtPuppet == true)
                    {
                        if (actor.GetComponent<ButtPuppet>().ApplySensors)
                            actor.GetComponent<ButtPuppet>().ApplySensors = false;

                        //Nasty loop. TODO: More performant solution. 
                        foreach (Transform child in PuppetReplay.transform.GetComponentsInChildren<Transform>())
                        {
                            if (child.name != JawJointMimic.name)
                            {
                                foreach (Transform nestedChild in actor.transform.GetComponentsInChildren<Transform>())
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
                        if (!actor.GetComponent<ButtPuppet>().ApplySensors)
                            actor.GetComponent<ButtPuppet>().ApplySensors = true;
                    }
                }

                /*if (OverwriteButtPuppet == true)
                    _OverwriteButtPuppet = true;
                else
                    _OverwriteButtPuppet = false;

                if (OverwriteJaw == true)
                    _OverwriteJaw = true;
                else
                    _OverwriteJaw = false;*/
            }
        }

        private void InitializeAnimation()
        {
            PuppetReplay = Instantiate(actor, actor.transform.position + new Vector3(0, 0, 2), Quaternion.identity);

            KillChildren(PuppetReplay.GetComponentsInChildren<MrPuppet.JawTransformMapper>());
            KillChildren(PuppetReplay.GetComponentsInChildren<MrPuppet.ButtPuppet>());
            KillChildren(PuppetReplay.GetComponentsInChildren<MrPuppet.Blink>());
            KillChildren(PuppetReplay.GetComponentsInChildren<MrPuppet.HeadPuppet>());
            KillChildren(PuppetReplay.GetComponentsInChildren<MrPuppet.JawBlendShapeMapper>());
            KillChildren(PuppetReplay.GetComponentsInChildren<MrPuppet.JointFollower>());
            KillChildren(PuppetReplay.GetComponentsInChildren<MrPuppet.LookAtTarget>());
            KillChildren(PuppetReplay.GetComponentsInChildren<MrPuppet.CaptureMicrophone>());
            KillChildren(PuppetReplay.GetComponentsInChildren<OneShotAnimations>());

            audioSource = PuppetReplay.AddComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.Play();

            Animator cloneReplay = PuppetReplay.AddComponent<Animator>();
            cloneReplay.runtimeAnimatorController = AnimatorController.CreateAnimatorControllerAtPathWithClip("Assets/Recordings/tempPAR.controller", animationClip);
            actor.GetComponent<MonoBehaviour>().StartCoroutine(StopAfterAnimation(cloneReplay));
            actor.transform.position = actor.transform.position + new Vector3(0, 0, 2);

            foreach (Transform child in PuppetReplay.transform.GetComponentsInChildren<Transform>())
            {
                if (child.name == JawJoint.name)
                {
                    JawJointMimic = child.gameObject;
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
                audioSource.Stop();
                Destroy(PuppetReplay);
                //AssetDatabase.DeleteAsset("Assets/Recordings/tempPAR.controller");

                OverwriteButtPuppet = false;
                OverwriteJaw = false;
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
            inCoroutine = true;

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
                        actor.GetComponent<MonoBehaviour>().StartCoroutine(StreamAudioClip(json.media_url));
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

        private IEnumerator StreamAudioClip(string url)
        {
            inCoroutine = true;
            using (UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
            {
                //infoBoxMsg = "LOADING AUDIO FILE - 0%";
                webRequest.SendWebRequest();

                while (!webRequest.isDone)
                {
                    //infoBoxMsg = "LOADING AUDIO FILE - " + Mathf.Round((webRequest.downloadProgress * 100)) + "%";
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
                    //infoBoxMsg = "The audio file has been succesfully loaded!";
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

    }
#else
public class AnimationPlayback : MonoBehaviour {

}
#endif
}
