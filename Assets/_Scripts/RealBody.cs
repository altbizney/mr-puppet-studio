using System;
using System.Collections.Generic;
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
            [Range(0, 1)]
            public float Pivot = 0f;
        }

        private class Bone
        {
            public Transform PivotTransf;
            public Transform BoneTransf;
        }
        
        public bool CreateArmParts;
        public bool SetLocalRotation;
        public Transform Root;
        public List<BodyJoint> PuppetJoints = new List<BodyJoint>();
        
        private readonly List<Bone> _bones = new List<Bone>();

        private void Start()
        {
            if(!CreateArmParts) return;
            
            for (var i = 0; i < PuppetJoints.Count; i++)
            {
                var pivot = new GameObject($"Bone{i}Pivot").transform;
                if (i > 0)
                    pivot.SetParent(_bones[i - 1].PivotTransf, false);
                else if (Root != null)
                {
                    pivot.SetParent(Root, true);
                    pivot.localPosition = Vector3.zero;
                }

                var bone = Instantiate(PuppetJoints[i].ModelPrefab).transform;
                bone.SetParent(pivot);

                _bones.Add(new Bone
                {
                    PivotTransf = pivot,
                    BoneTransf = bone
                });
            }
            
            AdjustBones();
            
            // Assign the newly created joints
            for (var i = 0; i < PuppetJoints.Count; i++)
            {
                PuppetJoints[i].Joint = _bones[i].PivotTransf;
            }
        }

        private void OnValidate()
        {
            AdjustBones();
        }

        private void AdjustBones()
        {
            for (var i = 0; i < _bones.Count; i++)
            {
                if (i > 0)
                {
                    _bones[i].PivotTransf.localPosition = new Vector3(0, -PuppetJoints[i - 1].Size.y);
                }
                
                _bones[i].BoneTransf.localScale = PuppetJoints[i].Size;
                _bones[i].BoneTransf.localPosition = new Vector3(0, (PuppetJoints[i].Pivot - .5f) * PuppetJoints[i].Size.y);
                
            }
        }

        private void Update()
        {
            foreach (var puppetJoint in PuppetJoints)
            {
                if (!puppetJoint.Enabled || puppetJoint.RealPuppetDataProvider == null || puppetJoint.Joint == null) continue;

                if (SetLocalRotation)
                {
                    puppetJoint.Joint.localRotation = Quaternion.Slerp(
                        puppetJoint.Joint.localRotation, 
                        Quaternion.Inverse(puppetJoint.Joint.parent.localRotation) * puppetJoint.RealPuppetDataProvider.GetInput(puppetJoint.InputSource) * Quaternion.Euler(puppetJoint.Offset), 
                        puppetJoint.Sharpness);
                }
                else
                {
                    puppetJoint.Joint.rotation = Quaternion.Slerp(
                        puppetJoint.Joint.rotation, 
                        puppetJoint.RealPuppetDataProvider.GetInput(puppetJoint.InputSource) * Quaternion.Euler(puppetJoint.Offset),// * Quaternion.Inverse(puppetJoint.TPose), 
                        puppetJoint.Sharpness);
                }
            }
        }
    }
}