using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class CountdownSlate : MonoBehaviour
{
    [Required]
    public Camera cam;

    public Texture2D card1;
    public Texture2D card2;
    public Texture2D card3;

    public AudioClip tone1;
    public AudioClip tone2;
    public AudioClip tone3;

    public RawImage image;

    public Transform DataTransportChannel;

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
            if (DataTransportChannel)
                DataTransportChannel.localPosition = new Vector3(DataTransportChannel.localPosition.x, 0f, DataTransportChannel.localPosition.z);
        }
        else if (time > 2f)
        {
            if (current != 1)
            {
                image.texture = card1;
                current = 1;
                AudioSource.PlayClipAtPoint(tone1, cam.transform.position);
                if (DataTransportChannel)
                    DataTransportChannel.localPosition = new Vector3(DataTransportChannel.localPosition.x, 1f, DataTransportChannel.localPosition.z);
            }
        }
        else if (time > 1f)
        {
            if (current != 2)
            {
                image.texture = card2;
                current = 2;
                AudioSource.PlayClipAtPoint(tone2, cam.transform.position);
                if (DataTransportChannel)
                    DataTransportChannel.localPosition = new Vector3(DataTransportChannel.localPosition.x, 2f, DataTransportChannel.localPosition.z);
            }
        }
        else
        {
            if (current != 3)
            {
                image.enabled = true;
                image.texture = card3;
                current = 3;
                AudioSource.PlayClipAtPoint(tone3, cam.transform.position);
                if (DataTransportChannel)
                    DataTransportChannel.localPosition = new Vector3(DataTransportChannel.localPosition.x, 3f, DataTransportChannel.localPosition.z);
            }
        }
    }

    [Button(ButtonSizes.Gigantic)]
    private void Clap()
    {
        started = Time.time;
    }
}
