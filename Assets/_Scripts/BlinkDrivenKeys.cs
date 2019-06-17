using System;
using UnityEditor;
using UnityEngine;

namespace Thinko
{
    [RequireComponent(typeof(DrivenKeys))]
    public class BlinkDrivenKeys : Blink
    {
        public DrivenKeys DrivenKeys;

        private void Start()
        {
            if (DrivenKeys == null)
            {
                Debug.LogWarning($"{name}:{GetType()} - No DrivenKeys defined");
                enabled = false;
            }
        }

        protected override void Update()
        {
            base.Update();
            
            DrivenKeys.Step = EyelidState;
        }
    }

    [CustomEditor(typeof(BlinkDrivenKeys))]
    public class BlinkDrivenKeysEditor : Editor
    {
        private void OnEnable()
        {
            var drivenKeys = target as BlinkDrivenKeys;
            drivenKeys.DrivenKeys = drivenKeys.GetComponent<DrivenKeys>();
        }
    }
}