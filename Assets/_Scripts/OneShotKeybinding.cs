using System;
using System.Collections;
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

        private void Awake () {
            _Animator = GetComponent<Animator> ();
            _OneShots = EditorWindow.GetWindow<OneShotsWindow> ();
        }

        private void OnEnable(){ FindObjectOfType<MrPuppetHubConnection>().OneShotDataEvent += HubConnectionSubsciption; }
        private void OnDisable(){ FindObjectOfType<MrPuppetHubConnection>().OneShotDataEvent -= HubConnectionSubsciption; }


        private void HubConnectionSubsciption (string SocketData) {
            if (!string.IsNullOrEmpty (SocketData)) {
                    DataSplit = SocketData.Split (';');
                    HubConnectionCommand = DataSplit[2];
                    HubConnectionCommand = new string (HubConnectionCommand.Where (c => !char.IsControl (c)).ToArray ());
            }
        }

        private void ChooseAnimation (int index) {
            //int randomIndex = UnityEngine.Random.Range (0, _OneShots.KeyCommands[index].Parameters.Count);
            //_OneShots.KeyCommands[index].Parameters[randomIndex] == OneShotsWindow.OneShotParameters.Wave

            if (index == 0) {
                if (CoroutineManager[0] != null) {
                    StopCoroutine (CoroutineManager[0]);
                    CoroutineManager[0] = null;
                }

                StartCoroutine (EaseForward (1));
                _Animator.SetTrigger ("WaveRightTrigger");
                CoroutineManager[2] = StartCoroutine (EaseBackward (3));
                HubConnectionCommand = "";
            }

            if (index == 1) {
                if (CoroutineManager[1] != null) {
                    StopCoroutine (CoroutineManager[1]);
                    CoroutineManager[1] = null;
                }

                StartCoroutine (EaseForward (2));
                _Animator.SetTrigger ("ThumbsUpTrigger");
                CoroutineManager[2] = StartCoroutine (EaseBackward (3));
                HubConnectionCommand = "";
            }

            if (index == 2) {

                if (CoroutineManager[2] != null) {
                    StopCoroutine (CoroutineManager[2]);
                    CoroutineManager[2] = null;
                }

                StartCoroutine (EaseForward (3));

                _Animator.SetTrigger ("GestureTrigger");

                if (_Animator.GetCurrentAnimatorStateInfo (1).IsName ("WaveRight") || _Animator.GetCurrentAnimatorStateInfo (1).IsName ("ONESHOT_Thinking")) {
                    CoroutineManager[0] = StartCoroutine (EaseBackward (1));
                }

                if (_Animator.GetCurrentAnimatorStateInfo (2).IsTag ("ThumbsUp")) {
                    CoroutineManager[1] = StartCoroutine (EaseBackward (2));
                }
                HubConnectionCommand = "";
            }
        }

        private void Update () {
            if (Input.GetKeyDown (_OneShots.KeyCommands[0].Key) || HubConnectionCommand == "Wave") {
                ChooseAnimation (0);
            }

            if (Input.GetKeyDown (_OneShots.KeyCommands[1].Key) || HubConnectionCommand == "ThumbsUp") {
                ChooseAnimation (1);
            }

            if (Input.GetKeyDown (_OneShots.KeyCommands[2].Key) || HubConnectionCommand == "Gesture") {
                ChooseAnimation (2);
            }
        }

        IEnumerator EaseBackward (int layer) {

            float currentLerp = _Animator.GetLayerWeight (layer);
            float t = currentLerp;

            while (t > 0) {
                currentLerp -= Time.deltaTime;

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
                currentLerp += Time.deltaTime * 2f;

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