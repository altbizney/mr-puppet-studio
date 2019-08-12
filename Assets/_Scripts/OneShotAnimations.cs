using System;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class OneShotAnimations : MonoBehaviour
{
    [Serializable]
    public class OneShotTrigger
    {
        public KeyCode TriggerKey = KeyCode.Space;
        [Required]
        public string TriggerName;
        [HideInInspector] public int TriggerHash;
    }

    public OneShotTrigger[] Triggers;

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        foreach (var trigger in Triggers)
        {
            trigger.TriggerHash = Animator.StringToHash(trigger.TriggerName);
        }
    }

    private void Update()
    {
        foreach (var trigger in Triggers)
        {
            if (Input.GetKeyDown(trigger.TriggerKey))
            {
                _animator.SetTrigger(trigger.TriggerHash);
            }
        }
    }
}
