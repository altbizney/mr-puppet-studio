using System;
using System.Collections.Generic;
using UnityEngine;

namespace Thinko
{
    public class DrivenKeys : MonoBehaviour
    {
        [Serializable]
        public class BlendShapeKey
        {
            public SkinnedMeshRenderer SkinnedMeshRenderer;
            public int BlendShapeIndex = 0;
            public float BlendShapeMin = 0;
            public float BlendShapeMax = 100;
        }
        
        [Serializable]
        public class TransformKey
        {
            public Transform Transform;
            public Vector3 InitPosition;
            public Vector3 EndPosition;
            public Quaternion InitRotation;
            public Quaternion EndRotation;
        }

        public List<BlendShapeKey> BlendShapeKeys = new List<BlendShapeKey>();
        public List<TransformKey> TransformKeys = new List<TransformKey>();

        [Range(0, 1)] 
        public float Step;
        
        private float _previousStep;

        private void Update()
        {
            if (Math.Abs(Step - _previousStep) > float.Epsilon)
            {
                _previousStep = Step;
                DoStep(Step);
            }
        }

        public void DoStep(float step)
        {
            foreach (var blendShapeKey in BlendShapeKeys)
            {
                if(blendShapeKey.BlendShapeIndex < blendShapeKey.SkinnedMeshRenderer.sharedMesh.blendShapeCount)
                    blendShapeKey.SkinnedMeshRenderer.SetBlendShapeWeight(blendShapeKey.BlendShapeIndex, step.Remap(0, 1, blendShapeKey.BlendShapeMin, blendShapeKey.BlendShapeMax));
            }

            foreach (var transformKey in TransformKeys)
            {
                if (transformKey.Transform == null) continue;
                transformKey.Transform.localPosition = Vector3.Lerp(transformKey.InitPosition, transformKey.EndPosition, step);
//                transformKey.Transform.rotation = Quaternion.Lerp(transformKey.InitRotation, transformKey.EndRotation, step);
            }
        }
    }
}