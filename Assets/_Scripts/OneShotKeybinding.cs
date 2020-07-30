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
        //Known bug when playing 9-7-9 in quick progression.
        //Can refactor this to on key down find the layer associated with that keypress, and do the weight set up they all need. 

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _Animator.SetLayerWeight(1, 1f);
            _Animator.SetTrigger("WaveRightTrigger");
            StartCoroutine(RampLayer(_Animator.GetLayerWeight(3), 3));
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _Animator.SetLayerWeight(1, 1f);
            _Animator.SetTrigger("ThinkingTrigger");
            StartCoroutine(RampLayer(_Animator.GetLayerWeight(3), 3));
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            _Animator.SetLayerWeight(2, 1f);
            _Animator.SetTrigger("ThumbsUpTrigger");

            Debug.Log(_Animator.GetLayerWeight(3));
            StartCoroutine(RampLayer(_Animator.GetLayerWeight(3), 3));
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            _Animator.SetTrigger("GestureTrigger");
            _Animator.SetLayerWeight(3, 1f);

            if (_Animator.GetCurrentAnimatorStateInfo(1).IsName("WaveRight") || _Animator.GetCurrentAnimatorStateInfo(1).IsName("ONESHOT_Thinking"))
            {
                StartCoroutine(RampLayer(_Animator.GetLayerWeight(1), 1));
            }

            if (_Animator.GetCurrentAnimatorStateInfo(2).IsTag("ThumbsUp"))
            {
                StartCoroutine(RampLayer(_Animator.GetLayerWeight(2), 2));
            }
        }

        /*
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
        */


        //if (Input.GetKeyDown(KeyCode.Alpha5))
        //    _Animator.SetTrigger("SpineWaveTrigger");

        //if (_Animator.GetBool("WaveRightTrigger") == true)
        //    Debug.Log("add");

    }

    IEnumerator Ease(float weight, int layer)
    {
        float currentLerp = 0f;
        float lerpTimer = 1f;
        float t = 0f;

        while (t < lerpTimer)
        {
            currentLerp += Time.deltaTime;
            if (currentLerp > lerpTimer)
            {
                currentLerp = lerpTimer;
            }

            t = currentLerp / lerpTimer;

            _Animator.SetLayerWeight(layer, t);

            yield return null;
        }
    }

    IEnumerator RampLayer(float weight, int layer)
    {
        for (float ft = weight; ft >= 0; ft -= 0.125f)
        {
            _Animator.SetLayerWeight(layer, ft);

            yield return null;
        }
    }

}

