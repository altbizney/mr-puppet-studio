﻿using System.Collections.Generic;
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

        public void OnEnable()
        {
            foreach (Renderer renderer in actor.GetComponentsInChildren<Renderer>())
            {
                if (renderer.enabled == true)
                    activeRenderers.Add(renderer);
            }
            infoBoxMsg = "The audio file has NOT been succesfully loaded yet...";
        }

        private GameObject clone;
        private List<Renderer> activeRenderers;
        private AudioClip audioClip;
        private AudioSource audioSource;
        private bool inCoroutine;
        private static string tempController = "temp.controller";

        [SerializeField]
        private GameObject actor;

        [SerializeField]
        [AssetSelector(Paths = "Assets/Recordings", FlattenTreeView = true)]
        private AnimationClip animationClip;

        [SerializeField]
        [BoxGroup]
        [DisplayAsString]
        [HideLabel]
        private string infoBoxMsg;

        [GUIColor(0.2f, 0.9f, 0.2f)]
        [ButtonGroup]
        [Button(ButtonSizes.Large)]
        [HideIf("IsPlaying")]
        [ShowIf("NotPlaying")]
        [DisableInEditorMode]
        private void PlayAnimation()
        {
            if (!audioClip && !inCoroutine)
                actor.GetComponent<MonoBehaviour>().StartCoroutine(StreamAudioClip());
            else
                InitilizeAnimation();
        }

        [GUIColor(0.9f, 0.3f, 0.3f)]
        [ButtonGroup]
        [Button(ButtonSizes.Large)]
        [HideIf("NotPlaying")]
        [ShowIf("IsPlaying")]
        [DisableInEditorMode]
        private void StopAnimation()
        {
            if (clone)
            {
                foreach (Renderer renderer in activeRenderers)
                    renderer.enabled = true;

                audioSource.Stop();

                Destroy(clone);
                AssetDatabase.DeleteAsset("Assets/Recordings/" + tempController);
            }
        }

        private void InitilizeAnimation()
        {
            if (!clone && audioClip.loadState == AudioDataLoadState.Loaded)
            {
                clone = Instantiate(actor, actor.transform.position, Quaternion.identity);

                KillChildren(clone.GetComponentsInChildren<JawTransformMapper>());
                KillChildren(clone.GetComponentsInChildren<ButtPuppet>());
                KillChildren(clone.GetComponentsInChildren<Blink>());
                KillChildren(clone.GetComponentsInChildren<HeadPuppet>());
                KillChildren(clone.GetComponentsInChildren<JawBlendShapeMapper>());
                KillChildren(clone.GetComponentsInChildren<JointFollower>());
                KillChildren(clone.GetComponentsInChildren<LookAtTarget>());
                KillChildren(clone.GetComponentsInChildren<CaptureMicrophone>());
                KillChildren(clone.GetComponentsInChildren<OneShotAnimations>());

                Animator cloneAnim = clone.AddComponent<Animator>();
                cloneAnim.enabled = true;
                cloneAnim.runtimeAnimatorController = AnimatorController.CreateAnimatorControllerAtPathWithClip("Assets/Recordings/" + tempController, animationClip);

                audioSource = actor.AddComponent<AudioSource>();
                audioSource.clip = audioClip;
                audioSource.Play();

                foreach (Renderer renderer in activeRenderers)
                    renderer.enabled = false;
            }
        }

        private void KillChildren(UnityEngine.Object[] children)
        {
            foreach (UnityEngine.Object child in children)
                Destroy(child);
        }

        // Potential refactor on these. Should be able to reduce to one line.
        // (Checking for null is a little expensive too, not that it matters too much right now)
        private bool NotPlaying()
        {
            if (clone == null)
                return true;
            else
                return false;
        }

        private bool IsPlaying()
        {
            if (clone == null)
                return false;
            else
                return true;
        }

        private IEnumerator QueryHyperMesh()
        {
            UnityWebRequest webRequest = UnityWebRequest.Get("https://hypermesh.app/renders/STARBY-E001-S001-cameraA-25-927:v1/info.json");
            webRequest.SetRequestHeader("Authorization", "Bearer eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJVc2VyOlZhbmhOSiJ9.3FXNh0U-DOJ76GwtFQJx1wblRzVFAPJElIrtN15pQEM");
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.Log(webRequest.error);
            }
            else
                Debug.Log(webRequest.downloadHandler.text);
        }

        private IEnumerator StreamAudioClip()
        {
            inCoroutine = true;
            using (UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip("https://hypermesh.accelerator.net/renders/QNgToB/audio", AudioType.WAV))
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
                    InitilizeAnimation();
                }
            }
        }

        [InitializeOnLoadAttribute]
        private static class PlayModeStateChanged
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
public class AnimationPlayback : MonoBehaviour {

}
#endif

}