using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using UnityEditor;

namespace MrPuppet
{
    [ExecuteInEditMode]
    public class FaceCapBlendShapeMapper : MonoBehaviour
    {
        [Serializable]
        public class BlendShapeMap
        {
            public enum FACSChannels
            {
                browInnerUp,
                browDown_L,
                browDown_R,
                browOuterUp_L,
                browOuterUp_R,
                eyeLookUp_L,
                eyeLookUp_R,
                eyeLookDown_L,
                eyeLookDown_R,
                eyeLookIn_L,
                eyeLookIn_R,
                eyeLookOut_L,
                eyeLookOut_R,
                eyeBlink_L,
                eyeBlink_R,
                eyeSquint_L,
                eyeSquint_R,
                eyeWide_L,
                eyeWide_R,
                cheekPuff,
                cheekSquint_L,
                cheekSquint_R,
                noseSneer_L,
                noseSneer_R,
                jawOpen,
                jawForward,
                jawLeft,
                jawRight,
                mouthFunnel,
                mouthPucker,
                mouthLeft,
                mouthRight,
                mouthRollUpper,
                mouthRollLower,
                mouthShrugUpper,
                mouthShrugLower,
                mouthClose,
                mouthSmile_L,
                mouthSmile_R,
                mouthFrown_L,
                mouthFrown_R,
                mouthDimple_L,
                mouthDimple_R,
                mouthUpperUp_L,
                mouthUpperUp_R,
                mouthLowerDown_L,
                mouthLowerDown_R,
                mouthPress_L,
                mouthPress_R,
                mouthStretch_L,
                mouthStretch_R,
                tongueOut
            };

            public ValueDropdownList<int> _BlendShapeNames = new ValueDropdownList<int>();
            private List<SkinnedMeshRenderer> _SkinnedMeshRenderers = new List<SkinnedMeshRenderer>();

            [ValueDropdown("_BlendShapeNames")] //showif
            public int BlendShape;

            public FACSChannels Channel;

            [ValueDropdown("_SkinnedMeshRenderers")]
            public SkinnedMeshRenderer _SkinnedMeshRenderer;


            public BlendShapeMap()
            {
                _SkinnedMeshRenderers = SkinnedMeshRenderers;
            }

            public void GetBlendShapeNames()
            {
                _BlendShapeNames.Clear();
                if (_SkinnedMeshRenderer)
                {
                    for (var i = 0; i < _SkinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                    {
                        _BlendShapeNames.Add(_SkinnedMeshRenderer.sharedMesh.GetBlendShapeName(i), i);
                    }
                }
            }
        }

        private static List<SkinnedMeshRenderer> SkinnedMeshRenderers = new List<SkinnedMeshRenderer>();
        public List<BlendShapeMap> Mappings = new List<BlendShapeMap>();

        //[Button(ButtonSizes.Small)]
        public void GetSkinnedMeshRenderers()
        {
            SkinnedMeshRenderers.Clear();

            foreach (Transform child in gameObject.GetComponentsInChildren<Transform>())
            {
                if (child.gameObject.GetComponent<SkinnedMeshRenderer>())
                {
                    SkinnedMeshRenderer childMesh = child.gameObject.GetComponent<SkinnedMeshRenderer>();
                    if (childMesh.sharedMesh.blendShapeCount > 0)
                        SkinnedMeshRenderers.Add(childMesh);
                }
            }
        }

        private void Update()
        {
            if (SkinnedMeshRenderers.Count == 0)
                GetSkinnedMeshRenderers();

            foreach (BlendShapeMap map in Mappings)
            {
                if (map._BlendShapeNames.Count == 0)
                    map.GetBlendShapeNames();
            }
        }

    }
}
