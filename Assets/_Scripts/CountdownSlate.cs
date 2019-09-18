using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class CountdownSlate : MonoBehaviour
{
    public Texture2D card1;
    public Texture2D card2;
    public Texture2D card3;

    public AudioClip tone1;
    public AudioClip tone2;
    public AudioClip tone3;

    public RawImage image;

    private float started = -1f;
    private int current = -1;

    private void Update()
    {
        if (started < 0) return;

        var time = (Time.time - started);

        if (time > 3f)
        {
            image.texture = null;
            image.enabled = false;
            started = -1f;
            current = -1;
        }
        else if (time > 2f)
        {
            if (current != 1)
            {
                image.texture = card1;
                AudioSource.PlayClipAtPoint(tone1, Camera.main.transform.position);
                current = 1;
            }
        }
        else if (time > 1f)
        {
            if (current != 2)
            {
                image.texture = card2;
                AudioSource.PlayClipAtPoint(tone2, Camera.main.transform.position);
                current = 2;
            }
        }
        else
        {
            if (current != 3)
            {
                image.enabled = true;
                image.texture = card3;
                AudioSource.PlayClipAtPoint(tone3, Camera.main.transform.position);
                current = 3;
            }
        }
    }

    [Button(ButtonSizes.Large)]
    private void Clap()
    {
        started = Time.time;
    }
}
