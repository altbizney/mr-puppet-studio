using UnityEngine;
using System.Collections.Generic;

namespace MrPuppet
{
    public class DemoCameraSwapper : MonoBehaviour
    {
        public GameObject HeroCamera;
        public GameObject PIPCamera;
        public GameObject WideCamera;

        public GameObject PIP1;
        public GameObject PIP2;
        public GameObject PIP3;
        public GameObject PIP4;
        public GameObject PIP5;
        public GameObject PIP6;
        public GameObject PIP7;
        public GameObject PIP8;

        private void Start()
        {
            HeroCamera.SetActive(true);
            PIPCamera.SetActive(false);
            WideCamera.SetActive(false);

            if (PIP1) PIP1.SetActive(false);
            if (PIP2) PIP2.SetActive(false);
            if (PIP3) PIP3.SetActive(false);
            if (PIP4) PIP4.SetActive(false);
            if (PIP5) PIP5.SetActive(false);
            if (PIP6) PIP6.SetActive(false);
            if (PIP7) PIP7.SetActive(false);
            if (PIP8) PIP8.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                HeroCamera.SetActive(true);
                PIPCamera.SetActive(false);
                WideCamera.SetActive(false);
                SwapPIP(null);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                HeroCamera.SetActive(false);
                PIPCamera.SetActive(true);
                WideCamera.SetActive(false);
                SwapPIP(PIP1);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                HeroCamera.SetActive(false);
                PIPCamera.SetActive(true);
                WideCamera.SetActive(false);
                SwapPIP(PIP2);
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                HeroCamera.SetActive(false);
                PIPCamera.SetActive(true);
                WideCamera.SetActive(false);
                SwapPIP(PIP3);
            }

            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                HeroCamera.SetActive(false);
                PIPCamera.SetActive(true);
                WideCamera.SetActive(false);
                SwapPIP(PIP4);
            }

            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                HeroCamera.SetActive(false);
                PIPCamera.SetActive(true);
                WideCamera.SetActive(false);
                SwapPIP(PIP5);
            }

            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                HeroCamera.SetActive(false);
                PIPCamera.SetActive(true);
                WideCamera.SetActive(false);
                SwapPIP(PIP6);
            }

            if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                HeroCamera.SetActive(false);
                PIPCamera.SetActive(true);
                WideCamera.SetActive(false);
                SwapPIP(PIP7);
            }

            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                HeroCamera.SetActive(false);
                PIPCamera.SetActive(true);
                WideCamera.SetActive(false);
                SwapPIP(PIP8);
            }

            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                HeroCamera.SetActive(false);
                PIPCamera.SetActive(false);
                WideCamera.SetActive(true);
                SwapPIP(null);
            }
        }

        private void SwapPIP(GameObject active = null)
        {
            if (PIP1) PIP1.SetActive(false);
            if (PIP2) PIP2.SetActive(false);
            if (PIP3) PIP3.SetActive(false);
            if (PIP4) PIP4.SetActive(false);
            if (PIP5) PIP5.SetActive(false);
            if (PIP6) PIP6.SetActive(false);
            if (PIP7) PIP7.SetActive(false);
            if (PIP8) PIP8.SetActive(false);

            if (active) active.SetActive(true);
        }
    }
}
