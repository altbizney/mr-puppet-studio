using System;
using System.Collections;
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

        private void Awake()
        {
            _Animator = GetComponent<Animator>();
            _OneShots = EditorWindow.GetWindow<OneShotsWindow>();

        }

        private void ChooseAnimation(int index)
        {
            int randomIndex = UnityEngine.Random.Range(0, _OneShots.KeyCommands[index].Parameters.Count);
            if (_OneShots.KeyCommands[index].Parameters[randomIndex] == OneShotsWindow.OneShotParameters.Wave)
                _Animator.SetTrigger("WaveRightTrigger");
            if (_OneShots.KeyCommands[index].Parameters[randomIndex] == OneShotsWindow.OneShotParameters.ThumbsUp)
                _Animator.SetTrigger("ThumbsUpTrigger");
            if (_OneShots.KeyCommands[index].Parameters[randomIndex] == OneShotsWindow.OneShotParameters.Gesture)
                _Animator.SetTrigger("GestureTrigger");
        }

        private void Update()
        {
            //These are all have similiar functionality. Refactor to eliminate repetition.

            if (Input.GetKeyDown(_OneShots.KeyCommands[0].Key))
            {
                if (CoroutineManager[0] != null)
                {
                    StopCoroutine(CoroutineManager[0]);
                    CoroutineManager[0] = null;
                }

                StartCoroutine(EaseForward(1));

                ChooseAnimation(0);

                CoroutineManager[2] = StartCoroutine(EaseBackward(3));
            }

            if (Input.GetKeyDown(_OneShots.KeyCommands[1].Key))
            {
                if (CoroutineManager[1] != null)
                {
                    StopCoroutine(CoroutineManager[1]);
                    CoroutineManager[1] = null;
                }

                StartCoroutine(EaseForward(2));

                ChooseAnimation(1);

                CoroutineManager[2] = StartCoroutine(EaseBackward(3));
            }

            if (Input.GetKeyDown(_OneShots.KeyCommands[2].Key))
            {
                if (CoroutineManager[2] != null)
                {
                    StopCoroutine(CoroutineManager[2]);
                    CoroutineManager[2] = null;
                }

                StartCoroutine(EaseForward(3));
                ChooseAnimation(2);

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
}
