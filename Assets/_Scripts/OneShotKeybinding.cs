using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
#endif

namespace MrPuppet
{
    public class OneShotKeybinding : MonoBehaviour
    {
        private Animator _Animator;
        private OneShotsWindow _OneShots;
        private Coroutine[] CoroutineManager = new Coroutine[3];
        private string[] DataSplit;
        private string HubConnectionCommand;
        private MrPuppetHubConnection HubConnection;

        private enum Layer { LArm, RArm, CArm }
        private Dictionary<string, Layer> TriggerLayerMap = new Dictionary<string, Layer>()
        { // trick linter not to wrap
            { "wave-right", Layer.RArm }, //
            { "thumbsup-left", Layer.LArm }, //
            { "gesture-1-both", Layer.CArm }, //
            { "gesture-3-right", Layer.RArm }, //
            { "fingerwag-left", Layer.LArm }, //
            { "gesture-2-left", Layer.LArm }, //
            { "peace-left", Layer.LArm }, //
            { "ok-left", Layer.LArm }, //
            { "gesture-4-both", Layer.CArm }, //
            { "gesture-5-both", Layer.CArm }, //
        };

        private void Awake()
        {
            _Animator = GetComponent<Animator>();
            _OneShots = EditorWindow.GetWindow<OneShotsWindow>();
        }

        private void OnEnable() { FindObjectOfType<MrPuppetHubConnection>().OneShotDataEvent += HubConnectionSubsciption; }
        private void OnDisable() { FindObjectOfType<MrPuppetHubConnection>().OneShotDataEvent -= HubConnectionSubsciption; }

        private void HubConnectionSubsciption(string SocketData)
        {
            if (!string.IsNullOrEmpty(SocketData))
            {
                DataSplit = SocketData.Split(';');
                HubConnectionCommand = DataSplit[2];
                HubConnectionCommand = new string(HubConnectionCommand.Where(c => !char.IsControl(c)).ToArray());
                StartOneShot(HubConnectionCommand);
            }
        }

        private void StartOneShot(string AccessName)
        {

            Layer AcessLayer = TriggerLayerMap[AccessName];
            int LayerIndex = 0;

            //Ease inactive layers out, so they can not interfere with the current animation
            //Find the index of the layer you are trying to access
            if (AcessLayer == Layer.RArm)
            {
                CoroutineManager[2] = StartCoroutine(EaseBackward(3));
                LayerIndex = 0;
            }
            else if (AcessLayer == Layer.LArm)
            {
                CoroutineManager[2] = StartCoroutine(EaseBackward(3));
                LayerIndex = 1;
            }
            else if (AcessLayer == Layer.CArm)
            {
                CoroutineManager[0] = StartCoroutine(EaseBackward(1));
                CoroutineManager[1] = StartCoroutine(EaseBackward(2));
                LayerIndex = 2;
            }

            //Stop any easing out of the layer you are currently trying to access
            if (CoroutineManager[LayerIndex] != null)
            {
                StopCoroutine(CoroutineManager[LayerIndex]);
                CoroutineManager[LayerIndex] = null;
            }

            //Ease in the layer you are accessing, so it can be displayed to the user
            StartCoroutine(EaseForward(LayerIndex + 1));
            _Animator.SetTrigger(AccessName);
            HubConnectionCommand = "";
        }

        private void Update()
        {
            foreach (var command in _OneShots.KeyCommands)
            {
                if (Input.GetKeyDown(command.Key))
                {
                    StartOneShot(command.Trigger);
                }
            }
        }

        IEnumerator EaseBackward(int layer)
        {

            float currentLerp = _Animator.GetLayerWeight(layer);
            float t = currentLerp;

            while (t > 0)
            {
                currentLerp -= Time.deltaTime * 1.5f;

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
                currentLerp += Time.deltaTime * 1.5f;

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
}
