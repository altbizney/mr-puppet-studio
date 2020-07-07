using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class OneShotTesting : MonoBehaviour
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
        if (Input.GetKeyDown("space"))
        {
            _Animator.SetTrigger("WaveRightTrigger");
            //_Animator.Play("test-wave-right", 1, 0f);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            _Animator.SetTrigger("PointLeftTrigger");
            //_Animator.Play("test-point-left", 2, 0f);
        }
    }
}