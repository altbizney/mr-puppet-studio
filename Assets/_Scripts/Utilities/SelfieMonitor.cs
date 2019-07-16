using UnityEngine;
using UnityEditor;

public class SelfieMonitor : EditorWindow
{
    private string RenderTextureAssetPath = "Assets/_Textures/SelfieRenderer.renderTexture";
    private float PlaybackFPS = 24f;
    private float LastFrameTime = 0.0f;

    private RenderTexture Texture;

    [MenuItem("Tools/Selfie Monitor")]
    private static void Init()
    {
        SelfieMonitor window = (SelfieMonitor)EditorWindow.GetWindow(typeof(SelfieMonitor));
        window.Show();
    }

    private void OnGUI()
    {
        if (Texture)
        {
            EditorGUI.DrawPreviewTexture(new Rect(position.width, 0f, -position.width, (Texture.height / Texture.width) * position.width), Texture);
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
    }

    private void OnDisable()
    {
        LastFrameTime = 0f;
    }
}