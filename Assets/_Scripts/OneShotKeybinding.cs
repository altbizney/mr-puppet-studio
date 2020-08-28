using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
#endif

namespace MrPuppet
{
    public class OneShotKeybinding : MonoBehaviour
    {
        private Animator _Animator;
        private OneShotsWindow _OneShots;
        private Coroutine[] CoroutineManager = new Coroutine[3];
        private string[] DataSplit;
        private MrPuppetHubConnection HubConnection;
        private string HubConnectionCommand;
        private enum Layer { LArm, RArm, CArm }

        Dictionary<string, Layer> TriggerLayerMap = new Dictionary<string, Layer>() {
            { "wave-right", Layer.RArm },
            { "thumbsup-left", Layer.LArm },
            { "gesture-1-both", Layer.CArm },
            { "gesture-3-right", Layer.RArm },
            { "fingerwag-left", Layer.LArm },
            { "gesture-2-left", Layer.LArm },
            { "peace-left", Layer.LArm },
            { "ok-left", Layer.LArm },
            { "gesture-4-both", Layer.CArm },
            { "gesture-5-both", Layer.CArm },
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

            if (CoroutineManager[LayerIndex] != null)
            {
                StopCoroutine(CoroutineManager[LayerIndex]);
                CoroutineManager[LayerIndex] = null;
            }

            StartCoroutine(EaseForward(LayerIndex + 1));
            _Animator.SetTrigger(AccessName);
            HubConnectionCommand = "";
        }

        private void Update()
        {
            if (Input.GetKeyDown(_OneShots.KeyCommands[0].Key))
            {
                StartOneShot("wave-right");
            }

            if (Input.GetKeyDown(_OneShots.KeyCommands[1].Key))
            {
                StartOneShot("thumbsup-left");
            }

            if (Input.GetKeyDown(_OneShots.KeyCommands[2].Key))
            {
                StartOneShot("gesture-1-both");
            }

            if (Input.GetKeyDown(_OneShots.KeyCommands[3].Key))
            {
                StartOneShot("gesture-3-right");
            }

            if (Input.GetKeyDown(_OneShots.KeyCommands[4].Key))
            {
                StartOneShot("fingerwag-left"); // arm goes down kinda fast
            }

            if (Input.GetKeyDown(_OneShots.KeyCommands[5].Key))
            {
                StartOneShot("gesture-2-left");
            }

            if (Input.GetKeyDown(_OneShots.KeyCommands[6].Key))
            {
                StartOneShot("peace-left");
            }

            if (Input.GetKeyDown(_OneShots.KeyCommands[7].Key))
            {
                StartOneShot("ok-left"); //arm goes up kinda fast?
            }

            if (Input.GetKeyDown(_OneShots.KeyCommands[8].Key))
            {
                StartOneShot("gesture-4-both");
            }

            if (Input.GetKeyDown(_OneShots.KeyCommands[9].Key))
            {
                StartOneShot("gesture-5-both");
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
