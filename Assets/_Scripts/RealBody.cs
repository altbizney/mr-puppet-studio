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
            [Range(0, 1)] public float Pivot = 0f;
            public Vector3 PivotDirection = new Vector3(-1, 0, 0);
        }

        private class Bone
        {
            public Transform PivotTransf;
            public Transform BoneTransf;
        }

        public bool CreateArmParts;
        public bool SetLocalRotation;
        public Transform Root;
        public List<BodyJoint> BodyJoints = new List<BodyJoint>();

        private readonly List<Bone> _bones = new List<Bone>();

        private void Start()
        {
            if (!CreateArmParts) return;

            for (var i = 0; i < BodyJoints.Count; i++)
            {
                var pivot = new GameObject($"Bone{i}Pivot").transform;
                if (i > 0)
                    pivot.SetParent(_bones[i - 1].PivotTransf, false);
                else if (Root != null)
                {
                    pivot.SetParent(Root, true);
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
                    var prevBodyJoint = BodyJoints[i - 1];
                    _bones[i].PivotTransf.localPosition = Vector3.Scale(new Vector3(
                            prevBodyJoint.Size.x - prevBodyJoint.Pivot * prevBodyJoint.Size.x,
                            prevBodyJoint.Size.y - prevBodyJoint.Pivot * prevBodyJoint.Size.y,
                            prevBodyJoint.Size.z - prevBodyJoint.Pivot * prevBodyJoint.Size.z),
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

        private void Update()
        {
            for (var i = 0; i < BodyJoints.Count; i++)
            {
                var bodyJoint = BodyJoints[i];
                if (!bodyJoint.Enabled || bodyJoint.RealPuppetDataProvider == null || bodyJoint.Joint == null) continue;

                var newRotation = SetLocalRotation ? bodyJoint.Joint.localRotation : bodyJoint.Joint.rotation;

                newRotation = Quaternion.Slerp(
                    newRotation,
                    bodyJoint.RealPuppetDataProvider.GetInput(bodyJoint.InputSource) * Quaternion.Euler(bodyJoint.Offset) * (i == 0 ? Quaternion.identity : Quaternion.Inverse(BodyJoints[i - 1].RealPuppetDataProvider.GetInput(BodyJoints[i - 1].InputSource))),
                    bodyJoint.Sharpness);

                if (SetLocalRotation)
                    bodyJoint.Joint.localRotation = newRotation;
                else
                    bodyJoint.Joint.rotation = newRotation;
            }
        }
    }
}