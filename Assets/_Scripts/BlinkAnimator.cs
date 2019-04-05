using UnityEngine;

[RequireComponent(typeof(Animation))]
public class BlinkAnimator : MonoBehaviour
{
    public Blink Blink;

    private AnimationState _animState;

    private void Awake()
    {
        var anim = GetComponent<Animation>();
        foreach (AnimationState state in anim)
        {
            _animState = state;
        }
    }

    private void Start()
    {
        if (Blink == null)
            enabled = false;
    }

    private void Update()
    {
        _animState.normalizedTime = Blink.EyelidState.Remap(1, 0, 0, 1);
    }
}
