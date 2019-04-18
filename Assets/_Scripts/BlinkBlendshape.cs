using UnityEngine;

public class BlinkBlendshape : MonoBehaviour
{
    public Blink Blink;

    public SkinnedMeshRenderer SkinnedMeshRenderer;

    public int BlendShapeIndex = 0;

    public float BlendShapeMin = 0;
    public float BlendShapeMax = 100;

    private void Start()
    {
        if (Blink == null || SkinnedMeshRenderer == null)
            enabled = false;
    }

    private void Update()
    {
        SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeIndex, Blink.EyelidState.Remap(0, 1, BlendShapeMin, BlendShapeMax));
    }
}
