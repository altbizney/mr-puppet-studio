using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class CountdownSlate : MonoBehaviour
{
    public Texture2D one;
    public Texture2D two;
    public Texture2D three;

    public RawImage image;

    private float started = -1f;

    private void Update()
    {
        if (started < 0) return;
        var time = (Time.time - started);

        if (time > 3f)
        {
            image.texture = null;
            image.enabled = false;
            started = -1f;
        }
        else if (time > 2f)
        {
            image.texture = one;
        }
        else if (time > 1f)
        {
            image.texture = two;
        }
        else
        {
            image.enabled = true;
            image.texture = three;
        }
    }

    [Button(ButtonSizes.Large)]
    private void Clap()
    {
        started = Time.time;
    }
}
