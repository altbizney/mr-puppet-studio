using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Thinko
{
    public class RealBody : MonoBehaviour
    {
        [Serializable]
        public class BodyJoint : RealPuppet.PuppetJoint
        {
            public GameObject ModelPrefab;
            public Vector3 Size = Vector3.one;
            [Range(0, 1)] public float Pivot = 0f;
            public Vector3 PivotDirection = new Vector3(-1, 0, 0);
            public Quaternion AttachPose;
        }

        private class Bone
        {
            public Transform PivotTransf;
            public Transform BoneTransf;
        }

        public bool CreateArmParts = true;

        public List<BodyJoint> BodyJoints = new List<BodyJoint>();

        private List<Bone> _bones = new List<Bone>();

        private void Start()
        {
            if (CreateArmParts)
                CreateArm();
        }

        private void OnValidate()
        {
            AdjustBones();
        }

        private void Update()
        {
            // Grab the TPose with keyboard
            if (Input.GetKeyDown(KeyCode.Space))
                GrabTPose();

            // Apply the rotations
            foreach (var bodyJoint in BodyJoints)
            {
                if (!bodyJoint.Enabled || bodyJoint.RealPuppetDataProvider == null || bodyJoint.Joint == null) continue;

                bodyJoint.Joint.rotation = Quaternion.Slerp(
                    bodyJoint.Joint.rotation,
                    bodyJoint.RealPuppetDataProvider.GetInput(bodyJoint.InputSource) * Quaternion.Euler(bodyJoint.Offset) * Quaternion.Inverse(bodyJoint.TPose),
                    bodyJoint.Sharpness);
            }
        }

        [Button(ButtonSizes.Large)]
        [GUIColor(0f, 1f, 0f)]
        private void GrabTPose()
        {
            foreach (var bodyJoint in BodyJoints)
            {
                if(bodyJoint.RealPuppetDataProvider != null)
                    bodyJoint.TPose = bodyJoint.RealPuppetDataProvider.GetInput(bodyJoint.InputSource);
            }
        }
        
        [Button(ButtonSizes.Large)]
        [GUIColor(0f, 1f, 0f)]
        private void GrabAttachPose()
        {
            foreach (var bodyJoint in BodyJoints)
            {
                if(bodyJoint.RealPuppetDataProvider != null)
                    bodyJoint.AttachPose = bodyJoint.RealPuppetDataProvider.GetInput(bodyJoint.InputSource);
            }
        }

        private void CreateArm()
        {
            for (var i = 0; i < BodyJoints.Count; i++)
            {
                var pivot = new GameObject($"Bone{i}Pivot").transform;
                if (i > 0)
                    pivot.SetParent(_bones[i - 1].PivotTransf, false);
                else
                {
                    pivot.SetParent(transform, true);
                    pivot.localPosition = Vector3.zero;
                }

                var bone = Instantiate(BodyJoints[i].ModelPrefab).transform;
                bone.SetParent(pivot);

                _bones.Add(new Bone
                {
                    PivotTransf = pivot,
                    BoneTransf = bone
                });
            }

            AdjustBones();

            // Assign the newly created joints
            for (var i = 0; i < BodyJoints.Count; i++)
            {
                BodyJoints[i].Joint = _bones[i].PivotTransf;
            }
        }
        
        private void AdjustBones()
        {
            for (var i = 0; i < _bones.Count; i++)
            {
                if (i > 0)
                {
                    var prevBodyJoint = BodyJoints[i - 1];
                    _bones[i].PivotTransf.localPosition = Vector3.Scale(new Vector3(
                            prevBodyJoint.Size.x,
                            prevBodyJoint.Size.y,
                            prevBodyJoint.Size.z),
                        prevBodyJoint.PivotDirection);
                }

                var bodyJoint = BodyJoints[i];
                _bones[i].BoneTransf.localScale = bodyJoint.Size;
                _bones[i].BoneTransf.localPosition = Vector3.Scale(new Vector3(
                        bodyJoint.Pivot * bodyJoint.Size.x,
                        bodyJoint.Pivot * bodyJoint.Size.y,
                        bodyJoint.Pivot * bodyJoint.Size.z),
                    bodyJoint.PivotDirection);
            }
        }
        
        private void OnDrawGizmos()
        {
            foreach (var bodyJoint in BodyJoints)
            {
                if(bodyJoint.RealPuppetDataProvider != null && bodyJoint.Joint != null && bodyJoint.Joint.gameObject.activeInHierarchy)
                {
                    var pos = bodyJoint.Joint.position;
                    var rot = bodyJoint.RealPuppetDataProvider.GetInput(bodyJoint.InputSource);
                    Debug.DrawRay(pos, rot * transform.forward, Color.blue, 0f, true);
                    Debug.DrawRay(pos, rot * transform.up, Color.green, 0f, true);
                    Debug.DrawRay(pos, rot * transform.right, Color.red, 0f, true);
                }
            }
        }
    }
}