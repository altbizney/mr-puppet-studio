using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;

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
            public FACSChannels Channel;

            private List<SkinnedMeshRenderer> _SkinnedMeshRenderers = new List<SkinnedMeshRenderer>();

            [ValueDropdown("_BlendShapeNames")]
            public int BlendShape;

            [ValueDropdown("_SkinnedMeshRenderers")]
            [OnValueChanged("ChangedSkinnedMesh")]
            public SkinnedMeshRenderer _SkinnedMeshRenderer;

            public BlendShapeMap()
            {
                _SkinnedMeshRenderers = SkinnedMeshRenderers;
                GetBlendShapeNames();
                if (_SkinnedMeshRenderers.Count > 0)
                    _SkinnedMeshRenderer = _SkinnedMeshRenderers[0];
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

            private void ChangedSkinnedMesh()
            {
                GetBlendShapeNames();
                if (_BlendShapeNames.Count > 0)
                    BlendShape = _BlendShapeNames[0].Value;
                //Debug.Log(_BlendShapeNames[0]);
                //Sirenix.Utilities.Editor.GUIHelper.RequestRepaint();
            }
        }

        private static List<SkinnedMeshRenderer> SkinnedMeshRenderers = new List<SkinnedMeshRenderer>();
        public List<BlendShapeMap> Mappings = new List<BlendShapeMap>();

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
            if (SkinnedMeshRenderers.Count == 0 || SkinnedMeshRenderers == null)
                GetSkinnedMeshRenderers();

            foreach (BlendShapeMap map in Mappings)
            {
                if (map._BlendShapeNames.Count == 0 || map._BlendShapeNames == null)
                    map.GetBlendShapeNames();

                if (map._SkinnedMeshRenderer == null)
                    map._SkinnedMeshRenderer = SkinnedMeshRenderers[0];
            }
        }
    }
}
