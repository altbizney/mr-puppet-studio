using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Formats.Fbx.Exporter;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
#endif

namespace MrPuppet
{
#if UNITY_EDITOR
    public class ExportPerformance : OdinEditorWindow
    {
        [MenuItem("Tools/Export Performances")]
        private static void OpenWindow()
        {
            GetWindow<ExportPerformance>().Show();
        }

        public GameObject Prefab;

        public List<AnimationClip> Clips;

        [Button(ButtonSizes.Large)]
        private void Export()
        {
            int success = 0;

            foreach (var clip in Clips)
            {
                // create controller
                var controller = AnimatorController.CreateAnimatorControllerAtPathWithClip("Assets/Recordings/" + clip.name + ".controller", clip);

                // create instance, rename
                var instance = GameObject.Instantiate(Prefab, Vector3.zero, Quaternion.identity);
                instance.name = clip.name;

                // add animator to instance
                var animator = instance.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;

                // export
                ModelExporter.ExportObject("Performances/" + clip.name + ".fbx", instance);

                // cleanup
                DestroyImmediate(instance);
                AssetDatabase.DeleteAsset("Assets/Recordings/" + clip.name + ".controller");
                FileUtil.MoveFileOrDirectory("Assets/Recordings/" + clip.name + ".anim", "Performances/" + clip.name + ".anim");

                success++;
            }

            if (success == Clips.Count)
            {
                Clips = new List<AnimationClip>();
            }
        }
    }
#else
public class ExportPerformance : MonoBehaviour {
    
}
#endif
}