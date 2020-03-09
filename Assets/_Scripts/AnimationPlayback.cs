using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class AnimationPlayback : MonoBehaviour
    {
        AudioSource audSource;

        GameObject clone;

        Animator anim;
        public AnimationClip animClip;
        public AudioClip audClip;

    void Start()
        {
            //anim = gameObject.GetComponent<Animator>();
            //anim.enabled = false;
            animClip.legacy = false;
            gameObject.GetComponent<MeshRenderer>().enabled = false;

            //rs = GetComponentsInChildren<Renderer>();
        }

    public void playAnim()
        {
            clone = Instantiate(gameObject, gameObject.transform.position, Quaternion.identity);

            Destroy(clone.GetComponent<MrPuppet.Blink>());
            Destroy(clone.GetComponent<MrPuppet.JawTransformMapper>());
            Destroy(clone.GetComponent<MrPuppet.ButtPuppet>());
            Destroy(clone.GetComponent<AnimationPlayback>());

            Animator cloneAnim = clone.GetComponent<Animator>();

            cloneAnim.enabled = true;

            AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController();
            animatorOverrideController.runtimeAnimatorController = cloneAnim.runtimeAnimatorController;
            animatorOverrideController["testAnim"] = animClip;
            cloneAnim.runtimeAnimatorController = animatorOverrideController;

            audSource = clone.AddComponent<AudioSource>();
            audSource.clip = audClip;
            audSource.Play();

            Renderer[] rs = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rs)
                r.enabled = false;

        //gameObject.GetComponent<MeshRenderer>().enabled = false;

    }

    public void stopAnim()
    {
        Renderer[] rs = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rs)
            r.enabled = true;

        Destroy(clone);
    }


        //insttiate the clone
        //deactivate all kinds of stuff from the clone, mainly puppet stuff. 
        //than just doo all the on the clone.
        //deactivate this actor? 

    //deactivate this script from clone?
    //add in some other script that will only ever run on clone? 

}
