using UnityEngine;
using UnityEngine.UI;

namespace MrPuppet
{
    public class DemoCameraSwapper : MonoBehaviour
    {
        public GameObject HeroCamera;
        public GameObject PIPCamera;
        public GameObject WideCamera;

        public GameObject GenericPIP;
        public GameObject CustomPIP;

        public KeyCode HeroKey = KeyCode.Alpha1;
        public KeyCode GenericPIPKey = KeyCode.Alpha2;
        public KeyCode CustomPIPKey = KeyCode.Alpha3;
        public KeyCode WideKey = KeyCode.Alpha4;

        private void Start()
        {
            HeroCamera.SetActive(true);
            PIPCamera.SetActive(false);
            WideCamera.SetActive(false);
            GenericPIP.SetActive(false);
            CustomPIP.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(HeroKey))
            {
                HeroCamera.SetActive(true);
                PIPCamera.SetActive(false);
                WideCamera.SetActive(false);
                GenericPIP.SetActive(false);
                CustomPIP.SetActive(false);
            }

            if (Input.GetKeyDown(GenericPIPKey))
            {
                HeroCamera.SetActive(false);
                PIPCamera.SetActive(true);
                WideCamera.SetActive(false);
                GenericPIP.SetActive(true);
                CustomPIP.SetActive(false);
            }

            if (Input.GetKeyDown(CustomPIPKey))
            {
                HeroCamera.SetActive(false);
                PIPCamera.SetActive(true);
                WideCamera.SetActive(false);
                GenericPIP.SetActive(false);
                CustomPIP.SetActive(true);
            }

            if (Input.GetKeyDown(WideKey))
            {
                HeroCamera.SetActive(false);
                PIPCamera.SetActive(false);
                WideCamera.SetActive(true);
                GenericPIP.SetActive(false);
                CustomPIP.SetActive(false);
            }
        }
    }
}
