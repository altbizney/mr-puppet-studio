using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Blink : MonoBehaviour
{
    public bool AutoBlink = true;
    
    [MinMaxSlider(.01f, 10f)]
    public Vector2 BlinkInterval = new Vector2(1, 4);
    
    [MinMaxSlider(.01f, .2f)]
    public Vector2 BlinkCloseDuration = new Vector2(.1f, .4f);
    
    [MinMaxSlider(.01f, .2f)]
    public Vector2 BlinkOpenDuration = new Vector2(.1f, .4f);
    
    [MinMaxSlider(.01f, .2f)]
    public Vector2 BlinkHoldDuration = new Vector2(.1f, .4f);

    public KeyCode ManualBlinkKey = KeyCode.Space;

    [ReadOnly]
    [Tooltip("1 is open, 0 is closed")]
    public float EyelidState = 1;

    private void OnEnable()
    {
        if (AutoBlink)
        {
            StartCoroutine(BlinkRoutine());
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(ManualBlinkKey))
        {
            StopAllCoroutines();
            StartCoroutine(TweenEyelidRoutine(Random.Range(BlinkCloseDuration.x, BlinkCloseDuration.y), 0));
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
            
            yield return StartCoroutine(TweenEyelidRoutine(Random.Range(BlinkCloseDuration.x, BlinkCloseDuration.y), 0));
            
            yield return new WaitForSeconds(Random.Range(BlinkHoldDuration.x, BlinkHoldDuration.y));
            
            yield return StartCoroutine(TweenEyelidRoutine(Random.Range(BlinkOpenDuration.x, BlinkOpenDuration.y), 1));
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
        yield return StartCoroutine(TweenEyelidRoutine(Random.Range(BlinkOpenDuration.x, BlinkOpenDuration.y), 1));
        
        if(AutoBlink)
            StartCoroutine(BlinkRoutine());
    }
}
