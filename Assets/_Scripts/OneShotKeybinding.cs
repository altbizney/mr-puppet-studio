using System;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;

public class OneShotKeybinding : MonoBehaviour
{
    private Animator _Animator;
    private Coroutine[] CoroutineManager = new Coroutine[3];

    private void Awake()
    {
        _Animator = GetComponent<Animator>();
    }

    private void Update()
    {
        //These are all have similiar functionality. Refactor to eliminate repetition. 

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (CoroutineManager[0] != null)
            {
                StopCoroutine(CoroutineManager[0]);
                CoroutineManager[0] = null;
            }

            StartCoroutine(EaseForward(1));
            _Animator.SetTrigger("WaveRightTrigger");

            CoroutineManager[2] = StartCoroutine(EaseBackward(3));
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (CoroutineManager[1] != null)
            {
                StopCoroutine(CoroutineManager[1]);
                CoroutineManager[1] = null;
            }

            StartCoroutine(EaseForward(2));

            _Animator.SetTrigger("ThumbsUpTrigger");

            CoroutineManager[2] = StartCoroutine(EaseBackward(3));
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (CoroutineManager[2] != null)
            {
                StopCoroutine(CoroutineManager[2]);
                CoroutineManager[2] = null;
            }

            StartCoroutine(EaseForward(3));
            _Animator.SetTrigger("GestureTrigger");

            if (_Animator.GetCurrentAnimatorStateInfo(1).IsName("WaveRight") || _Animator.GetCurrentAnimatorStateInfo(1).IsName("ONESHOT_Thinking"))
            {
                CoroutineManager[0] = StartCoroutine(EaseBackward(1));
            }

            if (_Animator.GetCurrentAnimatorStateInfo(2).IsTag("ThumbsUp"))
            {
                CoroutineManager[1] = StartCoroutine(EaseBackward(2));
            }
        }

        /*
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            _Animator.SetLayerWeight(1, 1f);
            _Animator.ResetTrigger("ThinkingTrigger");
            _Animator.SetTrigger("ThinkingTrigger");

            //myAnimationController.Play("animState", -1, normalizedTime = 0.0f);

            CArmCoroutine = StartCoroutine(Ease(_Animator.GetLayerWeight(3), 3));
        }

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
    }

    IEnumerator EaseBackward(int layer)
    {

        float currentLerp = _Animator.GetLayerWeight(layer);
        float t = currentLerp;

        while (t > 0)
        {
            currentLerp -= Time.deltaTime;

            if (currentLerp < 0)
            {
                currentLerp = 0;
            }

            t = currentLerp / 1;
            t = t * t * (3f - 2f * t);

            _Animator.SetLayerWeight(layer, t);

            yield return null;
        }
    }

    IEnumerator EaseForward(int layer)
    {

        float currentLerp = _Animator.GetLayerWeight(layer);
        float lerpTimer = 1f;
        float t = currentLerp;

        while (t < lerpTimer)
        {
            currentLerp += Time.deltaTime * 2f;

            if (currentLerp > lerpTimer)
            {
                currentLerp = lerpTimer;
            }

            t = currentLerp / lerpTimer;
            t = t * t * (3f - 2f * t);

            _Animator.SetLayerWeight(layer, t);

            yield return null;
        }
    }
}