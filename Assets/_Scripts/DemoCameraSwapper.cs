using UnityEngine;
using UnityEngine.UI;

namespace MrPuppet
{
    public class DemoCameraSwapper : MonoBehaviour
    {
        public GameObject HeroCamera;
        public GameObject PIPCamera;
        public GameObject WideCamera;

        public GameObject PIP;

        public KeyCode HeroKey = KeyCode.Alpha1;
        public KeyCode PIPKey = KeyCode.Alpha2;
        public KeyCode WideKey = KeyCode.Alpha3;

        private void Start()
        {
            HeroCamera.SetActive(true);
            PIPCamera.SetActive(false);
            WideCamera.SetActive(false);
            PIP.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(HeroKey))
            {
                HeroCamera.SetActive(true);
                PIPCamera.SetActive(false);
                WideCamera.SetActive(false);
                PIP.SetActive(false);
            }

            if (Input.GetKeyDown(PIPKey))
            {
                HeroCamera.SetActive(false);
                PIPCamera.SetActive(true);
                WideCamera.SetActive(false);
                PIP.SetActive(true);
            }

            if (Input.GetKeyDown(WideKey))
            {
                HeroCamera.SetActive(false);
                PIPCamera.SetActive(false);
                WideCamera.SetActive(true);
                PIP.SetActive(false);
            }
        }
    }
}