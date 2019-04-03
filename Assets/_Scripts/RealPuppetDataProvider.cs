using UnityEngine;

namespace Thinko
{
    public abstract class RealPuppetDataProvider : MonoBehaviour
    {
        [ReadOnly]
        public Quaternion Rotation;
        
        [ReadOnly]
        public float Jaw;
    }
}