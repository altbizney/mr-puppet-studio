using UnityEngine;

namespace Thinko
{
    public abstract class RealPuppetDataProvider : MonoBehaviour
    {
        [ReadOnly]
        public Quaternion Rotation;

        [ReadOnly]
        public Quaternion Rotation2;

        [ReadOnly]
        public float Jaw;
    }
}