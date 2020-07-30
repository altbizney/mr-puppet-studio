using System;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;

public class OneShotKeybinding : MonoBehaviour
{
    private Animator _Animator;

    private bool AnimationEndCheck(Animator animator)
    {
        if ((animator.GetCurrentAnimatorStateInfo(0).normalizedTime) % 1 < 0.99f)
            return true;
        else
            return false;
    }

    private void Awake()
    {
        _Animator = GetComponent<Animator>();
    }

    private void Update()
    {
        //layers can still get weird when you play a C animation, then play a L animatoin
        //L gets added to c still
        //if we ramp c down, then obv the animation will end. so that may be weird
        //we could potentially do an avatar mask, right? 

        //i see, the additiave animation continues. so if you play again anim is there
        //we dont wanna start ramping it down again. if its at 0 it should stay at 0

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _Animator.SetLayerWeight(1, 1f);
            _Animator.SetTrigger("WaveRightTrigger");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _Animator.SetLayerWeight(1, 1f);
            _Animator.SetTrigger("ThinkingTrigger");
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            _Animator.SetLayerWeight(2, 1f);
            _Animator.SetTrigger("ThumbsUpTrigger");
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            _Animator.SetTrigger("GestureTrigger");
            //if this animation is playing, set ramp weight to its weight
            Debug.Log(_Animator.GetCurrentAnimatorStateInfo(2).IsName("LArm"));

            if (_Animator.GetCurrentAnimatorStateInfo(1).IsName("WaveRight"))
            {
                StartCoroutine(RampLayer(_Animator.GetLayerWeight(1)));
            }

            Debug.Log(_Animator.GetCurrentAnimatorClipInfo(2)[0].clip.name);

            if (_Animator.GetCurrentAnimatorStateInfo(2).IsName("ThumbsUp"))
            {
                StartCoroutine(RampLayer(_Animator.GetLayerWeight(2)));
                Debug.Log("dajdlaj");
            }
        }


        // _Animator.SetTrigger("ShrugTrigger");
        if (Input.GetKeyDown(KeyCode.Alpha6))
            _Animator.SetTrigger("FingerWagTrigger");

        if (Input.GetKeyDown(KeyCode.Alpha8))
            _Animator.SetTrigger("HandsHipTrigger");

        if (Input.GetKeyDown(KeyCode.Alpha4))
            _Animator.SetTrigger("YawningTrigger");

        if (Input.GetKeyDown(KeyCode.Alpha5))
            _Animator.SetTrigger("ClapTrigger");

        if (Input.GetKeyDown(KeyCode.Alpha2))
            _Animator.SetTrigger("PointLeftTrigger");
        //when click gesture trigger
        //ramp weights down to 0

        //when click RArm trigger
        //ramp weights up to 1

        //if (Input.GetKeyDown(KeyCode.Alpha5))
        //    _Animator.SetTrigger("SpineWaveTrigger");

        //if (_Animator.GetBool("WaveRightTrigger") == true)
        //    Debug.Log("add");

    }

    IEnumerator RampLayer(float weight)
    {
        for (float ft = weight; ft >= 0; ft -= 0.15f)
        {
            _Animator.SetLayerWeight(1, ft);
            _Animator.SetLayerWeight(2, ft);

            yield return null;
        }
    }

}

