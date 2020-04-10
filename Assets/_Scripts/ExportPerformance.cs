using System.Collections.Generic;
using UnityEngine;
using System.IO;


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

        public enum Rating { Trash, Keeper, Blooper };

        public class ExportTake
        {
            public AnimationClip _Animation;
            public GameObject _Prefab;
            public Rating _Rating;

            public ExportTake(AnimationClip ConstructorAnimation, GameObject ConstructorPrefab, Rating ConstructorRating)
            {
                _Animation = ConstructorAnimation;
                _Prefab = ConstructorPrefab;
                _Rating = ConstructorRating;
            }
        }

        public List<ExportTake> Exports = new List<ExportTake>();

        [Button(ButtonSizes.Large)]
        private void Export()
        {
            int success = 0;

            foreach (var export in Exports)
            {
                if (export._Rating != Rating.Trash)
                {
                    // create controller
                    var controller = AnimatorController.CreateAnimatorControllerAtPathWithClip("Assets/Recordings/" + export._Animation.name + ".controller", export._Animation);

                    // create instance, rename
                    var instance = GameObject.Instantiate(export._Prefab, Vector3.zero, Quaternion.identity);
                    instance.name = export._Animation.name;

                    // add animator to instance
                    var animator = instance.AddComponent<Animator>();
                    animator.runtimeAnimatorController = controller;

                    // export
                    ModelExporter.ExportObject("Performances/" + export._Animation.name + ".fbx", instance);

                    // cleanup
                    DestroyImmediate(instance);
                    AssetDatabase.DeleteAsset("Assets/Recordings/" + export._Animation.name + ".controller");
                    FileUtil.MoveFileOrDirectory("Assets/Recordings/" + export._Animation.name + ".anim", "Performances/" + export._Animation.name + ".anim");
                }
                else
                {
                    FileUtil.MoveFileOrDirectory("Assets/Recordings/" + export._Animation.name + ".anim", "Performances/" + export._Animation.name + ".anim");
                }

                //write file
                var sr = File.CreateText("Performances/" + export._Animation.name + ".txt");
                sr.WriteLine(export._Animation.name + "," + export._Prefab.name + "," + export._Rating);
                sr.Close();

                success++;
            }

            if (success == Exports.Count)
            {
                Exports = new List<ExportTake>();
            }
        }
    }
#else
public class ExportPerformance : MonoBehaviour {

}
#endif
}
