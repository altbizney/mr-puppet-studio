using System.Collections;
using UnityEngine;

namespace Thinko
{
    public abstract class Blink : MonoBehaviour
    {
        public bool AutoBlink = true;

        [MinMaxSlider(.01f, 10f)] public Vector2 BlinkInterval = new Vector2(1, 3);

        [MinMaxSlider(.01f, .2f)] public Vector2 BlinkCloseDuration = new Vector2(.05f, .1f);

        [MinMaxSlider(.01f, .2f)] public Vector2 BlinkOpenDuration = new Vector2(.05f, .1f);

        [MinMaxSlider(.01f, .2f)] public Vector2 BlinkHoldDuration = new Vector2(.05f, .1f);

        public KeyCode ManualBlinkKey = KeyCode.B;

        public float OpenedValue = 1;
        public float ClosedValue = 0;

        [ReadOnly] public float EyelidState = 1;


        private void OnEnable()
        {
            if (AutoBlink)
            {
                EyelidState = OpenedValue;
                StartCoroutine(BlinkRoutine());
            }
        }

        private void Update()
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