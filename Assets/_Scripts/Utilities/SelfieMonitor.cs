using UnityEngine;
using UnityEditor;

public class SelfieMonitor : EditorWindow
{
    private float PlaybackFPS = 24f;
    private float LastFrameTime = 0.0f;

    private string RenderTextureAssetPath = "Assets/_Textures/SelfieRenderer.renderTexture";
    private RenderTexture Texture;

    private float imgRatio = 0f;
    private float containerRatio = 0f;
    private float finalWidth = 0f;
    private float finalHeight = 0f;

    // private MrPuppet.Blink Blinker;

    [MenuItem("Tools/Selfie Monitor")]
    private static void Init()
    {
        SelfieMonitor window = ScriptableObject.CreateInstance(typeof(SelfieMonitor)) as SelfieMonitor;
        window.Show();
    }

    private void OnGUI()
    {
        if (Texture)
        {
            // https://stackoverflow.com/a/10285523
            imgRatio = ((float)Texture.height / (float)Texture.width);
            containerRatio = (position.height / position.width);

            if (containerRatio > imgRatio)
            {
                finalHeight = position.height;
                finalWidth = (position.height / imgRatio);
            }
            else
            {
                finalWidth = position.width;
                finalHeight = (position.width * imgRatio);
            }

            EditorGUI.DrawPreviewTexture(new Rect(finalWidth, 0f, -finalWidth, finalHeight), Texture);
        }
        else
        {
            EditorGUILayout.LabelField(RenderTextureAssetPath + " not found");
        }
    }

    private void Update()
    {
        if (LastFrameTime < Time.time + (1f / PlaybackFPS))
        {
            LastFrameTime = Time.time;

            Repaint();
        }
    }

    private void OnEnable()
    {
        Texture = AssetDatabase.LoadAssetAtPath(RenderTextureAssetPath, typeof(RenderTexture)) as RenderTexture;
        // Blinker = GameObject.Find("lucius - idle").GetComponent<MrPuppet.Blink>();
    }

    private void OnDisable()
    {
        LastFrameTime = 0f;
    }
}
