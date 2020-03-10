
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

        //MonoBehaviour instance;

        private GameObject clone;
        private Renderer[] rs;
        private AudioClip audClip;
        private string tempController = "temp.controller";

        public AnimationClip animClip;
        public GameObject actor;

        [MenuItem("Tools/Animation Playback")]
        private static void OpenWindow()
        {
            GetWindow<AnimationPlayback>().Show();
        }

        IEnumerator streamAudioClip()
        {
            Debug.Log("Calling Coroutine");
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("https://hypermesh.accelerator.net/renders/QNgToB/audio", AudioType.OGGVORBIS))
            {
                yield return www.SendWebRequest();

                if (www.isNetworkError)
                    Debug.Log(www.error);
                else
                    audClip = DownloadHandlerAudioClip.GetContent(www);
            }
        }

        private void OnEnable()
        {
            //MonoBehaviour myMono = actor.GetComponent<MonoBehaviour>();
            //myMono.StartCoroutine(streamAudioClip());

            rs = actor.GetComponentsInChildren<Renderer>();
        }

        [GUIColor(0.2f, 0.9f, 0.2f)]
        [ButtonGroup]
        [Button(ButtonSizes.Large)]
        [DisableInEditorMode]
        private void playAnim()
        {
            if (!clone)
            {
                clone = Instantiate(actor, actor.transform.position, Quaternion.identity);

                Destroy(clone.GetComponent<Blink>());
                Destroy(clone.GetComponent<JawTransformMapper>());
                Destroy(clone.GetComponent<ButtPuppet>());

                var controller = AnimatorController.CreateAnimatorControllerAtPathWithClip("Assets/Recordings/"+tempController, animClip);

                Animator cloneAnim = clone.AddComponent<Animator>();

                cloneAnim.enabled = true;

                cloneAnim.runtimeAnimatorController = controller;

                AudioSource audSource = clone.AddComponent<AudioSource>();
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

                Destroy(clone);
                AssetDatabase.DeleteAsset("Assets/Recordings/"+tempController);
            }
        }
    }

#else
public class AnimationPlayback : MonoBehaviour {

}
#endif

    [InitializeOnLoadAttribute]
    public static class PlayModeStateChanged
    {
        static PlayModeStateChanged()
        {
            EditorApplication.playModeStateChanged += playModes;
        }

        private static void playModes(PlayModeStateChange state)
        {
            AssetDatabase.DeleteAsset("Assets/Recordings/temp.controller");
        }
    }

}
