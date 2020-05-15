using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet
{
    public class JawBlendShapeMapper : MonoBehaviour
    {
        [Serializable]
        public class BlendShapeMap
        {
            public ValueDropdownList<int> blendShapeNames = new ValueDropdownList<int>();

            [ValueDropdown("blendShapeNames"), LabelText("Blend Shape")]
            public int blendShapeIndex = 0;

            public float inputMin = 0f;
            public float inputMax = 1f;

            public float outputMin = 0f;
            public float outputMax = 1f;

            private float velocity = 0f;
            private float normalized = 0f;
            public float output = 0f;

            public bool spring = false;
            [ShowIf("spring"), MinValue(0f)]
            public float springStiffness = 100f;
            [ShowIf("spring"), MinValue(0f)]
            public float springDamping = 10f;

            public bool proxy = false;
            [ShowIf("proxy")]
            public Transform Proxy;

            public void Update(float driver, float smoothTime, SkinnedMeshRenderer skinnedMeshRenderer)
            {
                // remap input driver to output range
                normalized = Mathf.Lerp(outputMin, outputMax, Mathf.InverseLerp(inputMin, inputMax, driver));

                if (spring)
                {
                    output = SpringFloat(output, normalized, ref velocity, springStiffness, springDamping);
                }
                else
                {
                    output = Mathf.SmoothDamp(output, normalized, ref velocity, smoothTime);
                }

                if (blendShapeIndex < skinnedMeshRenderer.sharedMesh.blendShapeCount)
                {
                    skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, output);
                    if (proxy) Proxy.position = Vector3.Lerp(Vector3.zero, Vector3.one, output);
                }
            }

            // TODO: make generic/static
            private float SpringFloat(float current, float target, ref float velocity, float stiffness = 1f, float damping = 0.1f, float maxVelocity = Mathf.Infinity)
            {
                float dampingFactor = Mathf.Max(0f, 1f - damping * Time.smoothDeltaTime);
                float acceleration = (target - current) * stiffness * Time.smoothDeltaTime;
                velocity = velocity * dampingFactor + acceleration;

                if (maxVelocity < Mathf.Infinity)
                    velocity = Mathf.Clamp(velocity, -maxVelocity, maxVelocity);

                current += velocity * Time.smoothDeltaTime;

                if (Mathf.Abs(current - target) < 0.01f && Mathf.Abs(velocity) < 0.01f)
                {
                    current = target;
                    velocity = 0f;
                }

                return current;
            }
            // End fake generic/static
        }

        [Required]
        public MrPuppetDataMapper DataMapper;

        [Required]
        public SkinnedMeshRenderer skinnedMeshRenderer;

        [Range(0f, 0.5f)]
        public float smoothTime = 0.02f;

        [ReadOnly]
        [Range(0f, 1f)]
        public float SensorAmount = 0f;

        [ShowIf("skinnedMeshRenderer")]
        public List<BlendShapeMap> maps = new List<BlendShapeMap>();

        private void Awake() { SensorAmount = 0f; }

        void OnValidate()
        {
            if (!skinnedMeshRenderer) return;

            foreach (var map in maps)
            {
                map.blendShapeNames.Clear();
                for (var i = 0; i < skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
                {
                    map.blendShapeNames.Add(skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i), i);
                }
            }
        }

        void Update()
        {
            foreach (var map in maps)
            {
                map.Update(DataMapper.JawPercent * SensorAmount, smoothTime, skinnedMeshRenderer);
            }
        }
    }
}
