using Sirenix.OdinInspector;
using UnityEngine;

namespace Thinko.MrPuppet
{
    public class MrDummy : MonoBehaviour
    {
        [Required]
        public MrPuppetDataMapper DataMapper;

        public Transform ShoulderJoint;
        public Transform ElbowJoint;
        public Transform WristJoint;
        
        public Transform Jaw;
        
        private void Reset()
        {
            DataMapper = FindObjectOfType<MrPuppetDataMapper>();
        }
    }
}