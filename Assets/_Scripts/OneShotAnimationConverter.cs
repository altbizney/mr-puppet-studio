using UnityEngine;
using System.Collections;
using UnityEngine.Recorder;
using System.Linq;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor.Animations;
#endif

namespace MrPuppet
{
#if UNITY_EDITOR
    public class OneShotAnimationConverter : OdinEditorWindow
    {
        [MenuItem("Tools/One Shots Converter")]
        private static void OpenWindow()
        {
            GetWindow<OneShotAnimationConverter>().Show();
        }

        public class BlankMonoBehaviour : MonoBehaviour{ }

        public AnimatorController Controller;
        public GameObject Actor;
        public string AnimationName;

        private RecorderWindow Recorder;
        private GameObject Clone;
        private Coroutine AnimationCoroutine;
        private GameObject CoroutineHolder;
        static private string RecordedName;
        private bool StartRecording;

        //public AnimationClip _AnimationClip;
        //public Object Object2;

        private void Update()
        {
            if (Recorder == null)
                Recorder = EditorWindow.GetWindow<RecorderWindow>();

            if (EditorApplication.isPlaying && StartRecording == true && Recorder.IsRecording() && !Clone)
            {
                Clone = Instantiate(Actor, Actor.transform.position, Actor.transform.rotation);

                KillChildren(Clone.GetComponentsInChildren<JawTransformMapper>());
                KillChildren(Clone.GetComponentsInChildren<IKButtPuppet>());
                KillChildren(Clone.GetComponentsInChildren<Blink>());
                KillChildren(Clone.GetComponentsInChildren<FaceCapBlendShapeMapper>());
                KillChildren(Clone.GetComponentsInChildren<AudioReference>());

                SetRecorderTarget(Clone);

                Actor.SetActive(false);

                Animator animator = Clone.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.GetComponent<Animator>();
                animator.runtimeAnimatorController = Controller;
            }
            else
            {
                StartRecording = false;
            }
        }

/*
        [Button(ButtonSizes.Large)]
        private void TestRemove()
        {
            Avatar aa = AvatarBuilder.BuildGenericAvatar(Actor, "");
            aa.name = "TempAvatar";
            AssetDatabase.CreateAsset(aa, "Assets/new Avatar.asset");

            //AssetDatabase.AddObjectToAsset(aa, Object2);

            //Animator animator = Actor.GetComponent<Animator>() as Animator;
            //animator.avatar = aa;
        }
        C_Head_connector_JNT/R_Arm_Shoulder_connector_JNT/R_Arm_UpperArm_FK_connector_JNT
*/

        [Button(ButtonSizes.Large)]
        public void buildAvatarMask(){
            
            string name = "TestingAvatars";

            Avatar avatar = AvatarBuilder.BuildGenericAvatar(Actor,"");
            AvatarMask avatarMask = new AvatarMask();

            avatarMask.AddTransformPath(Actor.transform);

            AssetDatabase.CreateAsset(avatar, "Assets/" + name + ".anim.asset");
            AssetDatabase.CreateAsset(avatarMask, "Assets/" + name + ".mask.asset");
        }
 
        

        [DisableInEditorMode]
        [GUIColor(0.5f, 0.8f, 0.5f)]
        [Button(ButtonSizes.Large)]
        public void Convert()
        { 

                //CoroutineHolder = new GameObject("CoroutineHolder");
                //CoroutineHolder.AddComponent<BlankMonoBehaviour>();

                //AnimationCoroutine = Clone.GetComponent<MonoBehaviour>().StartCoroutine(AnimationEndCheck(_Animator));
                
                if (Recorder == null)
                    Recorder = EditorWindow.GetWindow<RecorderWindow>();
                
                Recorder.StartRecording();
                StartRecording = true;
        }

        [DisableInEditorMode]
        [GUIColor(0.8f, 0.5f, 0.5f)]
        [Button(ButtonSizes.Large)]
        public void Stop()
        { 
                if (Recorder == null)
                    Recorder = EditorWindow.GetWindow<RecorderWindow>();
                
                Recorder.StopRecording();
                AssetDatabase.RenameAsset("Assets/Recordings/" + RecordedName + ".anim", "ONESHOT." + AnimationName + ".anim");
                Destroy(Clone);
                Actor.SetActive(true);
                StartRecording = false;
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


                RecordedName = recorder.FileNameGenerator.FileName;
                RecordedName = RecordedName.Replace("<Take>", recorder.Take.ToString("000"));

                foreach (var input in recorder.InputsSettings) { ((AnimationInputSettings)input).gameObject = Clone; }

                return;
            }
        }

        private IEnumerator AnimationEndCheck(Animator animator)
        {
            while ((animator.GetCurrentAnimatorStateInfo(0).normalizedTime) % 1 < 0.9f){
                yield return null;
            }

            Recorder.StopRecording();
        }

    }
#else
    public class OneShotAnimationConverter : MonoBehaviour
    {

    }
#endif
}
