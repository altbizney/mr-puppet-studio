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

        private void OnEnable () { FindObjectOfType<MrPuppetHubConnection> ().OneShotDataEvent += HubConnectionSubsciption; }
        private void OnDisable () { FindObjectOfType<MrPuppetHubConnection> ().OneShotDataEvent -= HubConnectionSubsciption; }

        private void HubConnectionSubsciption (string SocketData) {
            if (!string.IsNullOrEmpty (SocketData)) {
                DataSplit = SocketData.Split (';');
                HubConnectionCommand = DataSplit[2];
                HubConnectionCommand = new string (HubConnectionCommand.Where (c => !char.IsControl (c)).ToArray ());
            }
            //When command is recieved. 
            //Call start oneshot animation method
            //Use string as parameter. Dont need to do any of this in update. 
        }

        /*
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
                CoroutineManager[0] = StartCoroutine (EaseBackward (1));
                CoroutineManager[1] = StartCoroutine (EaseBackward (2));

                HubConnectionCommand = "";
            }
        }
        */

        private void StartOneShot (string AcessName) {
            string[] SplitName = AcessName.Split ('-');
            string layer = SplitName[SplitName.Length - 1];
            //Debug.Log (layer);
            //This can be condensed and more optimized

            if (layer == "right") {
                if (CoroutineManager[0] != null) {
                    StopCoroutine (CoroutineManager[0]);
                    CoroutineManager[0] = null;
                }

                StartCoroutine (EaseForward (1));
                _Animator.SetTrigger (AcessName);
                CoroutineManager[2] = StartCoroutine (EaseBackward (3));
                HubConnectionCommand = "";
            }

            if (layer == "left") {
                if (CoroutineManager[1] != null) {
                    StopCoroutine (CoroutineManager[1]);
                    CoroutineManager[1] = null;
                }

                StartCoroutine (EaseForward (2));
                _Animator.SetTrigger (AcessName);
                CoroutineManager[2] = StartCoroutine (EaseBackward (3));
                HubConnectionCommand = "";
            }

            if (layer == "both") {

                if (CoroutineManager[2] != null) {
                    StopCoroutine (CoroutineManager[2]);
                    CoroutineManager[2] = null;
                }

                StartCoroutine (EaseForward (3));

                _Animator.SetTrigger (AcessName);
                CoroutineManager[0] = StartCoroutine (EaseBackward (1));
                CoroutineManager[1] = StartCoroutine (EaseBackward (2));

                HubConnectionCommand = "";
            }
        }

        private void Update () {
            if (Input.GetKeyDown (_OneShots.KeyCommands[0].Key) || HubConnectionCommand == "Wave") {
                StartOneShot ("wave-right");
            }

            if (Input.GetKeyDown (_OneShots.KeyCommands[1].Key) || HubConnectionCommand == "ThumbsUp") {
                StartOneShot ("thumbsup-left");
            }

            if (Input.GetKeyDown (_OneShots.KeyCommands[2].Key) || HubConnectionCommand == "Gesture") {
                StartOneShot ("gesture-1-both");
            }

            if (Input.GetKeyDown (_OneShots.KeyCommands[3].Key)) {
                StartOneShot ("gesture-3-right");
            }

            //////// new test each one further ////////

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