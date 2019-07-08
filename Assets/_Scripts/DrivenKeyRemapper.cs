using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thinko.MrPuppet
{
    public class DrivenKeyRemapper : MonoBehaviour
    {
        [Serializable]
        public class Key
        {
            public float inputMin = 0f;
            public float inputMax = 1f;

            public float outputMin = 0f;
            public float outputMax = 1f;

            public float output = 0f;

            public SkinnedMeshRenderer skinnedMeshRenderer;
            public int blendShapeIndex = 0;

            public void Step(float driver)
            {
                output = Mathf.Lerp(outputMin, outputMax, Mathf.InverseLerp(inputMin, inputMax, driver));

                if (blendShapeIndex < skinnedMeshRenderer.sharedMesh.blendShapeCount)
                {
                    skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, output);
                }
            }
        }

        [Range(0f, 1f)]
        public float driver = 0f;

        public List<Key> keys = new List<Key>();

        void Update()
        {
            foreach (var key in keys)
            {
                key.Step(driver);
            }
        }
    }
}
