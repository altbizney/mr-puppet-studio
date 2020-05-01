using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Linq;

namespace MrPuppet
{
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

            /*
            [HideLabel]
            public int MappingIndex;
            */

            [ValueDropdown("_SkinnedMeshRenderers", DropdownWidth = 200)]
            [OnValueChanged("ChangedSkinnedMesh")]
            [HorizontalGroup("SMR - BlendShape - Channel", MarginLeft = 0.01f, MarginRight = 0.01f)]
            [HideLabel]
            [TableColumnWidth(250)]
            public SkinnedMeshRenderer _SkinnedMeshRenderer;

            [ValueDropdown("_BlendShapeNames", DropdownWidth = 150)]
            [HorizontalGroup("SMR - BlendShape - Channel", MarginLeft = 0.01f, MarginRight = 0.01f)]
            [HideLabel]
            [TableColumnWidth(250)]
            public int BlendShape;

            [HideLabel]
            [HorizontalGroup("SMR - BlendShape - Channel", MarginLeft = 0.01f, MarginRight = 0.01f)]
            [TableColumnWidth(250)]
            public FACSChannels Channel;

            [Range(0f, 100f)]
            [OnValueChanged("SetBlendValue")]
            [DisableInPlayMode]
            [HideLabel]
            public float BlendValue = 0f;

            //maybe tiny bit less room for slider. set width of groups/tablelist?

            private void SetBlendValue()
            {
                //Debug.Log("Blend Value: " + BlendValue + " Blend Index: " + BlendShape + " SMR: " + _SkinnedMeshRenderer.name);
                _SkinnedMeshRenderer.SetBlendShapeWeight(BlendShape, BlendValue);
            }

            private List<SkinnedMeshRenderer> _SkinnedMeshRenderers()
            {
                return SkinnedMeshRenderers;
            }

            public BlendShapeMap()
            {
                GetBlendShapeNames();
                if (SkinnedMeshRenderers.Count > 0)
                    _SkinnedMeshRenderer = SkinnedMeshRenderers[0];
            }

            /*
            ~BlendShapeMap()
            {
                Debug.Log("deconstruct");
            }
            */

            public void GetBlendShapeNames()
            {
                _BlendShapeNames.Clear();

                if (_SkinnedMeshRenderer)
                {
                    for (var i = 0; i < _SkinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                    {
                        _BlendShapeNames.Add(_SkinnedMeshRenderer.sharedMesh.GetBlendShapeName(i).Substring(_SkinnedMeshRenderer.sharedMesh.GetBlendShapeName(i).IndexOf('.') + 1), i);
                    }
                }
            }

            private void ChangedSkinnedMesh()
            {
                GetBlendShapeNames();
                if (_BlendShapeNames.Count > 0)
                    BlendShape = _BlendShapeNames[0].Value;
            }
        }

        private static List<SkinnedMeshRenderer> SkinnedMeshRenderers = new List<SkinnedMeshRenderer>();
        [TableList]
        public List<BlendShapeMap> Mappings = new List<BlendShapeMap>();

        public void GetSkinnedMeshRenderers()
        {
            SkinnedMeshRenderers.Clear();

            foreach (SkinnedMeshRenderer child in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (child.sharedMesh.blendShapeCount > 0)
                {
                    SkinnedMeshRenderers.Add(child);
                }
            }
        }

        private void OnValidate()
        {
            if (SkinnedMeshRenderers == null || SkinnedMeshRenderers.Count == 0 || SkinnedMeshRenderers.Any(i => i == null))
            {
                GetSkinnedMeshRenderers();
            }

            foreach (BlendShapeMap map in Mappings)
            {
                if (!map._SkinnedMeshRenderer)
                {
                    map._SkinnedMeshRenderer = SkinnedMeshRenderers[0];
                }

                if (map._BlendShapeNames.Count == 0 || map._BlendShapeNames == null)
                    map.GetBlendShapeNames();
            }
        }


        [Button(ButtonSizes.Large)]
        public void AutoLoadMaps()
        {
            GetSkinnedMeshRenderers();

            foreach (SkinnedMeshRenderer smr in SkinnedMeshRenderers)
            {
                for (var i = 0; i < smr.sharedMesh.blendShapeCount; i++)
                {
                    string name = smr.sharedMesh.GetBlendShapeName(i).Substring(smr.sharedMesh.GetBlendShapeName(i).IndexOf('.') + 1);

                    foreach (BlendShapeMap.FACSChannels channel in Enum.GetValues(typeof(BlendShapeMap.FACSChannels)))
                    {
                        if (channel.ToString() != name) continue;
                        if (Mappings.Exists(x => x._SkinnedMeshRenderer == smr && x.Channel == channel)) continue;

                        BlendShapeMap map = new BlendShapeMap();
                        map.Channel = channel;
                        map._SkinnedMeshRenderer = smr;
                        map.BlendShape = i;
                        map.GetBlendShapeNames();
                        Mappings.Add(map);
                    }
                }

            }

        }
    }
}
