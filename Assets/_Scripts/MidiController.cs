using UnityEngine;
using MidiJack;

public class MidiController : MonoBehaviour
{
    // 3 9 12 13
    public int knobNumber = 3;
    public SkinnedMeshRenderer skinnedRenderer;
    public int blendShape = 0;

    [ReadOnly]
    public float knobPercent;

    void Awake()
    {

    }

    void Update()
    {
        knobPercent = MidiMaster.GetKnob(knobNumber);
        skinnedRenderer.SetBlendShapeWeight(blendShape, Mathf.Lerp(0f, 100f, knobPercent));
    }
}
