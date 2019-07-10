using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPuppet
{
    [RequireComponent(typeof(DrivenKeys))]
    public class Blink : MonoBehaviour
    {
        public bool AutoBlink = true;

        [MinMaxSlider(.01f, 10f)] public Vector2 BlinkInterval = new Vector2(1, 3);

        [MinMaxSlider(.01f, .2f)] public Vector2 BlinkCloseDuration = new Vector2(.05f, .1f);

        [MinMaxSlider(.01f, .2f)] public Vector2 BlinkOpenDuration = new Vector2(.05f, .1f);

        [MinMaxSlider(.01f, .2f)] public Vector2 BlinkHoldDuration = new Vector2(.05f, .1f);

        public bool Invert;

        public KeyCode ManualBlinkKey = KeyCode.B;

        private float OpenedValue => Invert ? 1 : 0;
        private float ClosedValue => Invert ? 0 : 1;

        [ReadOnly] public float EyelidState = 1;
        
        [Required] public DrivenKeys DrivenKeys;

        private void OnEnable()
        {
            if (AutoBlink)
            {
                EyelidState = OpenedValue;
                StartCoroutine(BlinkRoutine());
            }
        }

        protected virtual void Update()
        {
            if (Input.GetKeyDown(ManualBlinkKey))
            {
                StopAllCoroutines();
                StartCoroutine(TweenEyelidRoutine(Random.Range(BlinkCloseDuration.x, BlinkCloseDuration.y), ClosedValue));
            }

            if (Input.GetKeyUp(ManualBlinkKey))
            {
                StartCoroutine(ManualOpenRoutine());
            }
            
            DrivenKeys.Step = EyelidState;
        }

        private IEnumerator BlinkRoutine()
        {
            while (AutoBlink)
            {
                yield return new WaitForSeconds(Random.Range(BlinkInterval.x, BlinkInterval.y));

                yield return StartCoroutine(TweenEyelidRoutine(Random.Range(BlinkCloseDuration.x, BlinkCloseDuration.y), ClosedValue));

                yield return new WaitForSeconds(Random.Range(BlinkHoldDuration.x, BlinkHoldDuration.y));

                yield return StartCoroutine(TweenEyelidRoutine(Random.Range(BlinkOpenDuration.x, BlinkOpenDuration.y), OpenedValue));
            }
        }

        private IEnumerator TweenEyelidRoutine(float duration, float targetValue)
        {
            var t = 0f;
            while (t <= 1f)
            {
                t += Time.deltaTime / duration;

                EyelidState = Mathf.Lerp(EyelidState, targetValue, t);

                yield return null;
            }
        }

        private IEnumerator ManualOpenRoutine()
        {
            yield return StartCoroutine(TweenEyelidRoutine(Random.Range(BlinkOpenDuration.x, BlinkOpenDuration.y), OpenedValue));

            if (AutoBlink)
                StartCoroutine(BlinkRoutine());
        }
    }
}