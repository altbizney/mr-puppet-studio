using UnityEngine;
using UnityEditor.Recorder;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Recorder;
using UnityEditor.Recorder.Input;
//using UnityEngine.Recorder.Input;


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
        private RecorderWindow Recorder;
        private bool StartedRecording;
        private GameObject TransformWrapper;

        void Update()
        {
            if (Recorder == null)
                Recorder = EditorWindow.GetWindow<RecorderWindow>();

            if (Recorder.IsRecording() && StartedRecording == false)
            {
                StartedRecording = true;
                Action();
            }
            if (!Recorder.IsRecording() && StartedRecording == true)
            {
                StartedRecording = false;
            }
        }

        //[Button("Play")]
        //[DisableInEditorMode]
        private void Action()
        {
            GameObject Clone = Instantiate(Actor, new Vector3(0, 2f, 3f), Actor.transform.rotation);
            SetRecorderTarget(Clone);
            
            TransformWrapper = new GameObject("TransformWrapper");
            Clone.transform.parent = TransformWrapper.transform;
            TransformWrapper.transform.position += new Vector3(0, 0, 3.5f);

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

        private void SetRecorderTarget(GameObject Clone)
        {
            RecorderControllerSettings m_ControllerSettings = RecorderControllerSettings.LoadOrCreate(Application.dataPath + "/../Library/Recorder/recorder.pref");
            RecorderController m_RecorderController = new RecorderController(m_ControllerSettings);

            foreach (var recorder in m_RecorderController.Settings.RecorderSettings)
            {
                if (!recorder.Enabled) continue;

                foreach (var input in recorder.InputsSettings) { ((AnimationInputSettings)input).gameObject = Clone; }

                return;
            }
        }

    }
#else
    public class PerformanceAnimationReplacementManager : MonoBehaviour
    {

    }
#endif
}
