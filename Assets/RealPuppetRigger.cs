using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thinko
{

    [RequireComponent(typeof(RealPuppet))]
    public class RealPuppetRigger : MonoBehaviour
    {

        RealPuppet puppet;

        void Start()
        {

            puppet = GetComponent<RealPuppet>();
        }

        public void StartRiggingPuppet()
        {
            StartCoroutine(RigPuppet());

        }

        private IEnumerator RigPuppet()
        {
        
            yield return new WaitForEndOfFrame();


        }

    }
}