using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPlayback : MonoBehaviour
{
    AudioSource audSource;

    Animator anim;
    public AnimationClip animClip;
    public AudioClip audClip;

    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        anim.enabled = false;
        animClip.legacy = false;
    }

    public void playAnim()
    {
        anim.enabled = true;

        AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController();
        animatorOverrideController.runtimeAnimatorController = anim.runtimeAnimatorController;
        animatorOverrideController["testAnim"] = animClip;
        anim.runtimeAnimatorController = animatorOverrideController;

        audSource = gameObject.AddComponent<AudioSource>();
        audSource.clip = audClip;
        audSource.Play();

    }

}
