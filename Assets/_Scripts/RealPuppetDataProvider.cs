using UnityEngine;

namespace Thinko
{
    public abstract class RealPuppetDataProvider : MonoBehaviour
    {
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