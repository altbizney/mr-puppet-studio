
using System.Collections.Generic;
using UnityEngine;

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

        GameObject clone;

        bool playing = false;

        public AnimationClip animClip;
        public AudioClip audClip;
        public GameObject actor;

        [GUIColor(0.2f, 0.9f, 0.2f)]
        [ButtonGroup]
        [Button(ButtonSizes.Large)]
        [DisableInEditorMode]
        private void playAnim()
        {
            if (playing == false)
            {
                clone = Instantiate(actor, actor.transform.position, Quaternion.identity);

                Destroy(clone.GetComponent<MrPuppet.Blink>());
                Destroy(clone.GetComponent<MrPuppet.JawTransformMapper>());
                Destroy(clone.GetComponent<MrPuppet.ButtPuppet>());

                Animator cloneAnim = clone.GetComponent<Animator>();

                cloneAnim.enabled = true;

                AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController();
                animatorOverrideController.runtimeAnimatorController = cloneAnim.runtimeAnimatorController;
                animatorOverrideController["testAnim"] = animClip;
                cloneAnim.runtimeAnimatorController = animatorOverrideController;

                AudioSource audSource = clone.AddComponent<AudioSource>();
                audSource.clip = audClip;
                audSource.Play();

                Renderer[] rs = actor.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in rs)
                    r.enabled = false;

                playing = true;
            }

        }

        [GUIColor(0.9f, 0.3f, 0.3f)]
        [ButtonGroup]
        [Button(ButtonSizes.Large)]
        [DisableInEditorMode]
        private void stopAnim()
        {
            if (playing == true)
            {
                Renderer[] rs = actor.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in rs)
                    r.enabled = true;

                Destroy(clone);

                playing = false;
            }
        }

    }
#else
public class AnimationPlayback : MonoBehaviour {
    
}
#endif
}
