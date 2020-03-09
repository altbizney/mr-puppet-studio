
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnimationPlayback))]
public class customInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AnimationPlayback ac = (AnimationPlayback)target;
        if (GUILayout.Button("PLAY"))
        {
            ac.playAnim();
        }

    }

}
