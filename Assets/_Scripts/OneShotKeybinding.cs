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

namespace MrPuppet {
    public class OneShotKeybinding : MonoBehaviour {
        private Animator _Animator;
        private OneShotsWindow _OneShots;
        private Coroutine[] CoroutineManager = new Coroutine[3];
        private string[] DataSplit;
        private MrPuppetHubConnection HubConnection;
        private string HubConnectionCommand;
        Dictionary<string, int> TriggerLayerMap = new Dictionary<string, int> () 
        { 
            { "wave-right", 1 }, 
            { "thumbsup-left", 2 }, 
            { "gesture-1-both", 3 }, 
            { "gesture-3-right", 1 }, 
            { "fingerwag-left", 2 }, 
            { "gesture-2-left", 2 }, 
            { "peace-left", 2 }, 
            { "ok-left", 2 }, 
            { "gesture-4-both", 3 }, 
            { "gesture-5-both", 3 },
        };

        private void Awake () {
            _Animator = GetComponent<Animator> ();
            _OneShots = EditorWindow.GetWindow<OneShotsWindow> ();
        }

        private void OnEnable () { FindObjectOfType<MrPuppetHubConnection> ().OneShotDataEvent += HubConnectionSubsciption; }
        private void OnDisable () { FindObjectOfType<MrPuppetHubConnection> ().OneShotDataEvent -= HubConnectionSubsciption; }

        private void HubConnectionSubsciption (string SocketData) {
            if (!string.IsNullOrEmpty (SocketData)) {
                DataSplit = SocketData.Split (';');
                HubConnectionCommand = DataSplit[2];
                HubConnectionCommand = new string (HubConnectionCommand.Where (c => !char.IsControl (c)).ToArray ());
                StartOneShot (HubConnectionCommand);
            }
        }

        private void StartOneShot (string AccessName) {

            int layerIndex = TriggerLayerMap[AccessName];

            if (layerIndex == 1) {
                if (CoroutineManager[0] != null) {
                    StopCoroutine (CoroutineManager[0]);
                    CoroutineManager[0] = null;
                }

                StartCoroutine (EaseForward (1));
                _Animator.SetTrigger (AccessName);
                CoroutineManager[2] = StartCoroutine (EaseBackward (3));
                HubConnectionCommand = "";
            }

            if (layerIndex == 2) {
                if (CoroutineManager[1] != null) {
                    StopCoroutine (CoroutineManager[1]);
                    CoroutineManager[1] = null;
                }

                StartCoroutine (EaseForward (2));
                _Animator.SetTrigger (AccessName);
                CoroutineManager[2] = StartCoroutine (EaseBackward (3));
                HubConnectionCommand = "";
            }

            if (layerIndex == 3) {

                if (CoroutineManager[2] != null) {
                    StopCoroutine (CoroutineManager[2]);
                    CoroutineManager[2] = null;
                }

                StartCoroutine (EaseForward (3));

                _Animator.SetTrigger (AccessName);
                CoroutineManager[0] = StartCoroutine (EaseBackward (1));
                CoroutineManager[1] = StartCoroutine (EaseBackward (2));

                HubConnectionCommand = "";
            }
        }

        private void Update () {
            if (Input.GetKeyDown (_OneShots.KeyCommands[0].Key)) {
                StartOneShot ("wave-right");
            }

            if (Input.GetKeyDown (_OneShots.KeyCommands[1].Key)) {
                StartOneShot ("thumbsup-left");
            }

            if (Input.GetKeyDown (_OneShots.KeyCommands[2].Key)) {
                StartOneShot ("gesture-1-both");
            }

            if (Input.GetKeyDown (_OneShots.KeyCommands[3].Key)) {
                StartOneShot ("gesture-3-right");
            }

            if (Input.GetKeyDown (_OneShots.KeyCommands[4].Key)) {
                StartOneShot ("fingerwag-left"); // arm goes down kinda fast
            }

            if (Input.GetKeyDown (_OneShots.KeyCommands[5].Key)) {
                StartOneShot ("gesture-2-left");
            }

            if (Input.GetKeyDown (_OneShots.KeyCommands[6].Key)) {
                StartOneShot ("peace-left");
            }

            if (Input.GetKeyDown (_OneShots.KeyCommands[7].Key)) {
                StartOneShot ("ok-left"); //arm goes up kinda fast?
            }

            if (Input.GetKeyDown (_OneShots.KeyCommands[8].Key)) {
                StartOneShot ("gesture-4-both");
            }

            if (Input.GetKeyDown (_OneShots.KeyCommands[9].Key)) {
                StartOneShot ("gesture-5-both");
            }
        }

        IEnumerator EaseBackward (int layer) {

            float currentLerp = _Animator.GetLayerWeight (layer);
            float t = currentLerp;

            while (t > 0) {
                currentLerp -= Time.deltaTime * 1.5f;

                if (currentLerp < 0) {
                    currentLerp = 0;
                }

                t = currentLerp / 1;
                t = t * t * (3f - 2f * t);

                _Animator.SetLayerWeight (layer, t);

                yield return null;
            }
        }

        IEnumerator EaseForward (int layer) {

            float currentLerp = _Animator.GetLayerWeight (layer);
            float lerpTimer = 1f;
            float t = currentLerp;

            while (t < lerpTimer) {
                currentLerp += Time.deltaTime * 1.5f;

                if (currentLerp > lerpTimer) {
                    currentLerp = lerpTimer;
                }

                t = currentLerp / lerpTimer;
                t = t * t * (3f - 2f * t);

                _Animator.SetLayerWeight (layer, t);

                yield return null;
            }
        }
    }
}