
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;



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
    public class AnimationPlayback : OdinEditorWindow
    {

        [MenuItem("Tools/Animation Playback")]
        private static void OpenWindow()
        {
            GetWindow<AnimationPlayback>().Show();
        }

        private GameObject clone;
        private Renderer[] rs;
        private AudioClip audClip;
        private AudioSource audSource;
        private bool inCoroutine;
        private static string tempController = "temp.controller";


        public AnimationClip animClip;
        public GameObject actor;

        [BoxGroup]
        [DisplayAsString]
        [HideLabel]
        public string infoBoxMsg;

        public void OnEnable()
        {
            rs = actor.GetComponentsInChildren<Renderer>();
            infoBoxMsg = "The audio file has NOT been succesfully loaded yet...";
        }

        IEnumerator streamAudioClip()
        {
            inCoroutine = true;
            using (UnityWebRequest webR = UnityWebRequestMultimedia.GetAudioClip("https://hypermesh.accelerator.net/renders/QNgToB/audio", AudioType.WAV))
            {
                //((DownloadHandlerAudioClip)webR.downloadHandler).streamAudio = false;
                //Debug.Log("Start Audio Download");
                infoBoxMsg = "LOADING AUDIO FILE";

                yield return webR.SendWebRequest();

                /* while (webR.downloadProgress < 0.01)
                {
                    Debug.Log(webR.downloadProgress);
                    yield return new WaitForSeconds(.1f);
                }*/

                if (webR.isNetworkError)
                {
                    Debug.Log(webR.error);
                }
                else
                {

                    audClip = DownloadHandlerAudioClip.GetContent(webR);
                    //Debug.Log("Audio Download Completed");
                    inCoroutine = false;
                    infoBoxMsg = "The audio file has been succesfully loaded!";
                    initAnim();
                }
            }
        }

        [GUIColor(0.2f, 0.9f, 0.2f)]
        [ButtonGroup]
        [Button(ButtonSizes.Large)]
        [DisableInEditorMode]
        private void playAnim()
        {
            if (!audClip && !inCoroutine)
            {
                MonoBehaviour myMono = actor.GetComponent<MonoBehaviour>();
                myMono.StartCoroutine(streamAudioClip());
            }
            else
                initAnim();
        }

        private void initAnim()
        {
            if (!clone && audClip.loadState == AudioDataLoadState.Loaded)
            {
                clone = Instantiate(actor, actor.transform.position, Quaternion.identity);

                killChildren(clone.GetComponentsInChildren<JawTransformMapper>());
                killChildren(clone.GetComponentsInChildren<ButtPuppet>());
                killChildren(clone.GetComponentsInChildren<Blink>());
                killChildren(clone.GetComponentsInChildren<HeadPuppet>());
                killChildren(clone.GetComponentsInChildren<JawBlendShapeMapper>());
                killChildren(clone.GetComponentsInChildren<JointFollower>());
                killChildren(clone.GetComponentsInChildren<LookAtTarget>());
                killChildren(clone.GetComponentsInChildren<MrPuppetDataMapper>());
                killChildren(clone.GetComponentsInChildren<MrPuppetHubConnection>());
                killChildren(clone.GetComponentsInChildren<CaptureMicrophone>());
                killChildren(clone.GetComponentsInChildren<OneShotAnimations>());

                var controller = AnimatorController.CreateAnimatorControllerAtPathWithClip("Assets/Recordings/" + tempController, animClip);
                Animator cloneAnim = clone.AddComponent<Animator>();
                cloneAnim.enabled = true;
                cloneAnim.runtimeAnimatorController = controller;

                audSource = actor.AddComponent<AudioSource>();
                audSource.clip = audClip;
                audSource.Play();

                foreach (Renderer r in rs)
                    r.enabled = false;
            }

        }

        [GUIColor(0.9f, 0.3f, 0.3f)]
        [ButtonGroup]
        [Button(ButtonSizes.Large)]
        [DisableInEditorMode]
        private void stopAnim()
        {
           if (clone)
            {
                foreach (Renderer r in rs)
                    r.enabled = true;

                audSource.Stop();

                Destroy(clone);
                AssetDatabase.DeleteAsset("Assets/Recordings/"+tempController);
            }
        }

        [InitializeOnLoadAttribute]
        public static class PlayModeStateChanged
        {
            static PlayModeStateChanged()
            {
                EditorApplication.playModeStateChanged += playModes;
            }

            private static void playModes(PlayModeStateChange state)
            {
                AssetDatabase.DeleteAsset("Assets/Recordings/"+tempController);
            }
        }

        private void killChildren(UnityEngine.Object[] children)
        {
            foreach (UnityEngine.Object child in children)
                Destroy(child);
        }

    }
#else
public class AnimationPlayback : MonoBehaviour {

}
#endif

}
