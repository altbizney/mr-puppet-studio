using System;
using UnityEngine;

namespace Thinko
{
    public abstract class RealPuppetDataProvider : MonoBehaviour
    {
        public enum Source
        {
            Wrist,
            Elbow,
            Shoulder
        }

        public Quaternion GetInput(Source source)
        {
            var q = Quaternion.identity;
            switch (source)
            {
                case Source.Wrist:
                    q = WristRotation;
                    break;
                case Source.Elbow:
                    q = ElbowRotation;
                    break;
                case Source.Shoulder:
                    q = ShoulderRotation;
                    break;
            }

            return q;
        }
        
        [ReadOnly]
        public Quaternion WristRotation;
        
        [ReadOnly]
        public Quaternion ElbowRotation;
        
        [ReadOnly]
        public Quaternion ShoulderRotation;
        
        [ReadOnly]
        public float Jaw;
    }
}