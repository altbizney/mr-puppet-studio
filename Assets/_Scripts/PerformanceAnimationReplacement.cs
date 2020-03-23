using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;


namespace MrPuppet
{
    public class PerformanceAnimationReplacement : MonoBehaviour
    {

        //duplicate the object it is create on twice

        private GameObject CloneAnimation;
        private GameObject CloneMerge;

        public Transform JawJoint;
        private GameObject CloneMergeJawJoint;

        public AnimationClip animationClip;

        void Start()
        {
            CloneAnimation = Instantiate(gameObject, gameObject.transform.position + new Vector3(0, 0, 2), Quaternion.identity);
            CloneMerge = Instantiate(gameObject, gameObject.transform.position - new Vector3(0, 0, 2), Quaternion.identity);

            //Probably just Instatiate the clone that already has its children removed, so you dont have to do it twice
            KillChildren(CloneAnimation.GetComponentsInChildren<MrPuppet.PerformanceAnimationReplacement>());
            KillChildren(CloneAnimation.GetComponentsInChildren<MrPuppet.JawTransformMapper>());
            KillChildren(CloneAnimation.GetComponentsInChildren<MrPuppet.ButtPuppet>());
            KillChildren(CloneAnimation.GetComponentsInChildren<MrPuppet.Blink>());
            KillChildren(CloneAnimation.GetComponentsInChildren<MrPuppet.HeadPuppet>());
            KillChildren(CloneAnimation.GetComponentsInChildren<MrPuppet.JawBlendShapeMapper>());
            KillChildren(CloneAnimation.GetComponentsInChildren<MrPuppet.JointFollower>());
            KillChildren(CloneAnimation.GetComponentsInChildren<MrPuppet.LookAtTarget>());
            KillChildren(CloneAnimation.GetComponentsInChildren<MrPuppet.CaptureMicrophone>());
            KillChildren(CloneAnimation.GetComponentsInChildren<OneShotAnimations>());

            KillChildren(CloneMerge.GetComponentsInChildren<MrPuppet.PerformanceAnimationReplacement>());
            KillChildren(CloneMerge.GetComponentsInChildren<MrPuppet.JawTransformMapper>());
            KillChildren(CloneMerge.GetComponentsInChildren<MrPuppet.ButtPuppet>());
            KillChildren(CloneMerge.GetComponentsInChildren<MrPuppet.Blink>());
            KillChildren(CloneMerge.GetComponentsInChildren<MrPuppet.HeadPuppet>());
            KillChildren(CloneMerge.GetComponentsInChildren<MrPuppet.JawBlendShapeMapper>());
            KillChildren(CloneMerge.GetComponentsInChildren<MrPuppet.JointFollower>());
            KillChildren(CloneMerge.GetComponentsInChildren<MrPuppet.LookAtTarget>());
            KillChildren(CloneMerge.GetComponentsInChildren<MrPuppet.CaptureMicrophone>());
            KillChildren(CloneMerge.GetComponentsInChildren<OneShotAnimations>());

            Animator cloneAnim = CloneAnimation.AddComponent<Animator>();
            cloneAnim.runtimeAnimatorController = AnimatorController.CreateAnimatorControllerAtPathWithClip("Assets/Recordings/tempPAR.controller", animationClip);
            gameObject.transform.position = gameObject.transform.position + new Vector3(0, 0, 2);

            //Find clones Jaw, to apply tranformations too. 
            foreach (Transform child in CloneMerge.transform.GetComponentsInChildren<Transform>())
            {
                if (child.name == JawJoint.name)
                {
                    CloneMergeJawJoint = child.gameObject;
                }
            }

        }

        private void LateUpdate()
        {
          CloneMergeJawJoint.transform.rotation = JawJoint.rotation;

            //Nasty loop. Will find a way to create a more performant solution. 
            foreach (Transform child in CloneAnimation.transform.GetComponentsInChildren<Transform>())
            {
                if (child.name != CloneMergeJawJoint.name)
                {
                    foreach (Transform nestedChild in CloneMerge.transform.GetComponentsInChildren<Transform>())
                    {
                        if (nestedChild.name == child.name)
                        {
                            nestedChild.rotation = child.rotation;
                            //probs position too. 
                        }
                    }
                }
            }
        }

        private void KillChildren(UnityEngine.Object[] children)
        {
            foreach (UnityEngine.Object child in children)
                Destroy(child);
        }
    }
}