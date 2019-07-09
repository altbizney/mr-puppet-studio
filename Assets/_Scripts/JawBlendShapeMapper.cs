using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thinko.MrPuppet
{
    public class JawBlendShapeMapper : MonoBehaviour
    {
        [Serializable]
        public class BlendShapeMap
        {
            public float inputMin = 0f;
            public float inputMax = 1f;

            public float outputMin = 0f;
            public float outputMax = 1f;

            private float velocity = 0f;
            private float output = 0f;

            // TODO: nice UI for picking blend shape
            public SkinnedMeshRenderer skinnedMeshRenderer;
            public int blendShapeIndex = 0;

            public void Update(float driver, float smoothTime)
            {
                output = Mathf.SmoothDamp(output, Mathf.Lerp(outputMin, outputMax, Mathf.InverseLerp(inputMin, inputMax, driver)), ref velocity, smoothTime);

                if (blendShapeIndex < skinnedMeshRenderer.sharedMesh.blendShapeCount)
                {
                    skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, output);
                }
            }
        }

        // TODO: read from e.g. DataMapper
        public RealBody MrPuppet;

        public List<BlendShapeMap> maps = new List<BlendShapeMap>();

        [Range(0f, 0.5f)]
        public float smoothTime = 0.01f;

        void Update()
        {
            foreach (var map in maps)
            {
                map.Update(MrPuppet.JawPercent, smoothTime);
            }
        }
    }
}
