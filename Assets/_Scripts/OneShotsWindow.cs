using UnityEngine;
using UnityEditor.Recorder;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor.Animations;
#endif

namespace MrPuppet
{
#if UNITY_EDITOR
    public class OneShotsWindow : OdinEditorWindow
    {

        [MenuItem("Tools/One Shots Performance")]
        private static void OpenWindow()
        {
            GetWindow<OneShotsWindow>().Show();
        }

        public GameObject Actor;
        public AnimationClip Performance;

        void Update()
        {
            if (Recorder == null)
                Recorder = EditorWindow.GetWindow<RecorderWindow>();

            if (Recorder.IsRecording() && StartedRecording == false)
            {
                StartedRecording = true;
                GetFilename();
            }
            if (!Recorder.IsRecording() && StartedRecording == true)
            {
                StartedRecording = false;
                var filename = "Assets/Recordings/" + Filename + ".anim";
                Exports.Add(new ExportPerformance.ExportTake((AnimationClip)AssetDatabase.LoadAssetAtPath(filename, typeof(AnimationClip)), RecorderTarget, Rating.Keeper));
                RecorderPrompt.ShowUtilityWindow(this);
            }
        }

        [Button("Play")]
        [DisableInEditorMode]
        private void Action()
        {
            //Start when recorder starts?
            //What do when base animation ends? 

            GameObject Clone = Instantiate(Actor, new Vector3(0, 0, 0), Actor.transform.rotation);

            KillChildren(Clone.GetComponentsInChildren<MrPuppet.JawTransformMapper>());
            KillChildren(Clone.GetComponentsInChildren<MrPuppet.ButtPuppet>());
            KillChildren(Clone.GetComponentsInChildren<MrPuppet.IKButtPuppet>());
            KillChildren(Clone.GetComponentsInChildren<MrPuppet.Blink>());
            KillChildren(Clone.GetComponentsInChildren<MrPuppet.HeadPuppet>());
            KillChildren(Clone.GetComponentsInChildren<MrPuppet.JawBlendShapeMapper>());
            KillChildren(Clone.GetComponentsInChildren<MrPuppet.JointFollower>());
            KillChildren(Clone.GetComponentsInChildren<MrPuppet.LookAtTarget>());
            KillChildren(Clone.GetComponentsInChildren<MrPuppet.CaptureMicrophone>());
            KillChildren(Clone.GetComponentsInChildren<OneShotAnimations>());
            KillChildren(Clone.GetComponentsInChildren<IKTag>());
            KillChildren(Clone.GetComponentsInChildren<Animator>());
            //There are more components on the clone that are potentially unnessary


            Animator AnimatorTemplate = Clone.AddComponent<Animator>(); 

            AssetDatabase.CopyAsset("Assets/Resources/OneShots.controller", "Assets/Resources/OneShotsTemp.controller");
            AnimatorTemplate.runtimeAnimatorController =  Resources.Load("OneShotsTemp") as RuntimeAnimatorController;

            AnimatorOverrideController AnimatorOverride = new AnimatorOverrideController(AnimatorTemplate.runtimeAnimatorController);
            AnimatorTemplate.runtimeAnimatorController = AnimatorOverride;
            AnimatorOverride["BaseAnimation"] = Performance;
            Clone.AddComponent<OneShotTesting>(); 

            //AssetDatabase.DeleteAsset("Assets/Resources/OneShotsTemp.controller");
        }

        private void KillChildren(UnityEngine.Object[] children)
        {
            foreach (UnityEngine.Object child in children)
                Destroy(child);
        }
    }
#else
    public class PerformanceAnimationReplacementManager : MonoBehaviour
    {

    }
#endif
}
