using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thinko.MrPuppet
{
    public class JawBlendShapeMapper : MonoBehaviour
    {
        [Serializable]
        public class BlendShape
        {
            public float inputMin = 0f;
            public float inputMax = 1f;

            public float outputMin = 0f;
            public float outputMax = 1f;

            private float output = 0f;

            // TODO: nice UI for picking blend shape
            public SkinnedMeshRenderer skinnedMeshRenderer;
            public int blendShapeIndex = 0;

            public void Update(float driver)
            {
                output = Mathf.Lerp(outputMin, outputMax, Mathf.InverseLerp(inputMin, inputMax, driver));

                if (blendShapeIndex < skinnedMeshRenderer.sharedMesh.blendShapeCount)
                {
                    skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, output);
                }
            }
        }

        // TODO: read from e.g. DataMapper
        public RealBody MrPuppet;

        public List<BlendShape> maps = new List<BlendShape>();

        void Update()
        {
            foreach (var map in maps)
            {
                map.Update(MrPuppet.JawPercent);
            }
        }
    }
}
