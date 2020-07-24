using System;
using Sirenix.OdinInspector;
using UnityEngine;

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
        if (Input.GetKeyDown(KeyCode.Alpha1))
            _Animator.SetTrigger("WaveRightTrigger");

        if (Input.GetKeyDown(KeyCode.Alpha2))
            _Animator.SetTrigger("PointLeftTrigger");

        if (Input.GetKeyDown(KeyCode.Alpha3))
            _Animator.SetTrigger("ThinkingTrigger");

        if (Input.GetKeyDown(KeyCode.Alpha4))
            _Animator.SetTrigger("YawningTrigger");

        if (Input.GetKeyDown(KeyCode.Alpha5))
            _Animator.SetTrigger("ClapTrigger");

        if (Input.GetKeyDown(KeyCode.Alpha6))
            _Animator.SetTrigger("FingerWagTrigger");

        if (Input.GetKeyDown(KeyCode.Alpha7))
            _Animator.SetTrigger("GestureTrigger");

        if (Input.GetKeyDown(KeyCode.Alpha8))
            _Animator.SetTrigger("HandsHipTrigger");

        if (Input.GetKeyDown(KeyCode.Alpha9))
            _Animator.SetTrigger("ShrugTrigger");

        //if (Input.GetKeyDown(KeyCode.Alpha5))
        //    _Animator.SetTrigger("SpineWaveTrigger");

        //if (_Animator.GetBool("WaveRightTrigger") == true)
        //    Debug.Log("add");

    }

    //can you detect if triggger is true?
    //if so, just check all of those, find the words you liie, light em up as the animation plays. 

}

