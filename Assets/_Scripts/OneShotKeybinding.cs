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
        if (Input.GetKeyDown(KeyCode.N))
        {
            _Animator.SetTrigger("WaveRightTrigger");
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            _Animator.SetTrigger("PointLeftTrigger");
        }
    }
}