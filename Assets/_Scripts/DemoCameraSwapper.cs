using System;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace MrPuppet
{
    public class DemoCameraSwapper : MonoBehaviour
    {
        public GameObject HeroCamera;
        public GameObject PIPCamera;
        public GameObject WideCamera;

        public GameObject LowerThird;
        public GameObject PIP1;
        public GameObject PIP2;
        public GameObject PIP3;
        public GameObject PIP4;
        public GameObject PIP5;
        public GameObject PIP6;
        public GameObject PIP7;
        public GameObject PIP8;

        public KeyCode PrevView = KeyCode.UpArrow;
        public KeyCode NextView = KeyCode.DownArrow;
        private int CurrentView = 1;
        private string GraphicsRoot;

        private void Start()
        {
            SwapGraphics(1);

            GraphicsRoot = Path.Combine(Application.persistentDataPath, "Graphics");

            DownloadGraphics();
        }

        private void DownloadGraphics()
        {
            StartCoroutine(LoadGraphic("LowerThird.png", LowerThird));
            StartCoroutine(LoadGraphic("PIP1.png", PIP1));
            StartCoroutine(LoadGraphic("PIP2.png", PIP2));
            StartCoroutine(LoadGraphic("PIP3.png", PIP3));
            StartCoroutine(LoadGraphic("PIP4.png", PIP4));
            StartCoroutine(LoadGraphic("PIP5.png", PIP5));
            StartCoroutine(LoadGraphic("PIP6.png", PIP6));
            StartCoroutine(LoadGraphic("PIP7.png", PIP7));
            StartCoroutine(LoadGraphic("PIP8.png", PIP8));
        }

        private IEnumerator LoadGraphic(string filename, GameObject GO)
        {
            RawImage image = GO.GetComponent<RawImage>();

            if (File.Exists(Path.Combine(GraphicsRoot, filename)))
            {
                UnityWebRequest www = UnityWebRequestTexture.GetTexture("file:///" + Path.Combine(GraphicsRoot, filename));
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log("Error downloading " + filename + ": " + www.error);
                }
                else
                {
                    image.texture = DownloadHandlerTexture.GetContent(www);
                    image.texture.wrapMode = TextureWrapMode.Clamp;
                    image.texture.filterMode = FilterMode.Bilinear;
                    image.SetNativeSize();
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SwapGraphics(1);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SwapGraphics(2);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SwapGraphics(3);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SwapGraphics(4);
            if (Input.GetKeyDown(KeyCode.Alpha5)) SwapGraphics(5);
            if (Input.GetKeyDown(KeyCode.Alpha6)) SwapGraphics(6);
            if (Input.GetKeyDown(KeyCode.Alpha7)) SwapGraphics(7);
            if (Input.GetKeyDown(KeyCode.Alpha8)) SwapGraphics(8);
            if (Input.GetKeyDown(KeyCode.Alpha9)) SwapGraphics(9);
            if (Input.GetKeyDown(KeyCode.Alpha0)) SwapGraphics(0);

            if (Input.GetKeyDown(PrevView))
            {
                if (CurrentView == 0) CurrentView = 9;
                else if (CurrentView != 1) CurrentView -= 1;
                SwapGraphics(CurrentView);
            }

            if (Input.GetKeyDown(NextView))
            {
                if (CurrentView == 9) CurrentView = 0;
                else if (CurrentView != 0) CurrentView += 1;
                SwapGraphics(CurrentView);
            }
        }

        private void SwapGraphics(int view)
        {
            if (PIP1) PIP1.SetActive(false);
            if (PIP2) PIP2.SetActive(false);
            if (PIP3) PIP3.SetActive(false);
            if (PIP4) PIP4.SetActive(false);
            if (PIP5) PIP5.SetActive(false);
            if (PIP6) PIP6.SetActive(false);
            if (PIP7) PIP7.SetActive(false);
            if (PIP8) PIP8.SetActive(false);

            if (view == 0)
            {
                HeroCamera.SetActive(false);
                PIPCamera.SetActive(false);
                WideCamera.SetActive(true);

                CurrentView = 0;
            }

            if (view == 1)
            {
                HeroCamera.SetActive(true);
                PIPCamera.SetActive(false);
                WideCamera.SetActive(false);

                CurrentView = 1;
            }

            if (view == 2)
            {
                HeroCamera.SetActive(false);
                PIPCamera.SetActive(true);
                WideCamera.SetActive(false);

                if (PIP1) PIP1.SetActive(true);

                CurrentView = 2;
            }

            if (view == 3)
            {
                HeroCamera.SetActive(false);
                PIPCamera.SetActive(true);
                WideCamera.SetActive(false);

                if (PIP2) PIP2.SetActive(true);

                CurrentView = 3;
            }

            if (view == 4)
            {
                HeroCamera.SetActive(false);
                PIPCamera.SetActive(true);
                WideCamera.SetActive(false);

                if (PIP3) PIP3.SetActive(true);

                CurrentView = 4;
            }

            if (view == 5)
            {
                HeroCamera.SetActive(false);
                PIPCamera.SetActive(true);
                WideCamera.SetActive(false);

                if (PIP4) PIP4.SetActive(true);

                CurrentView = 5;
            }

            if (view == 6)
            {
                HeroCamera.SetActive(false);
                PIPCamera.SetActive(true);
                WideCamera.SetActive(false);

                if (PIP5) PIP5.SetActive(true);

                CurrentView = 6;
            }

            if (view == 7)
            {
                HeroCamera.SetActive(false);
                PIPCamera.SetActive(true);
                WideCamera.SetActive(false);

                if (PIP6) PIP6.SetActive(true);

                CurrentView = 7;
            }

            if (view == 8)
            {
                HeroCamera.SetActive(false);
                PIPCamera.SetActive(true);
                WideCamera.SetActive(false);

                if (PIP7) PIP7.SetActive(true);

                CurrentView = 8;
            }

            if (view == 9)
            {
                if (PIP8) PIP8.SetActive(true);

                CurrentView = 9;
            }
        }
    }
}
