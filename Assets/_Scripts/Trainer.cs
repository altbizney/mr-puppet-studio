using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MrPuppet
{
    public class Trainer : MonoBehaviour
    {
        public MrPuppetHubConnection HubConnection;

        public TMP_Dropdown CameraDropdown;
        public Button TPoseButton;
        public Button AttachButton;
        public Button JawOpenButton;
        public Button JawCloseButton;

        public GameObject BurtCamera;
        public GameObject CliveCamera;
        public GameObject LuciusCamera;

        private void OnValidate()
        {
            if (HubConnection == null) HubConnection = FindObjectOfType<MrPuppetHubConnection>();
        }

        private void Update()
        {
            CameraDropdown.interactable = HubConnection.IsConnected;
            TPoseButton.interactable = HubConnection.IsConnected;
            AttachButton.interactable = HubConnection.IsConnected;
            JawOpenButton.interactable = HubConnection.IsConnected;
            JawCloseButton.interactable = HubConnection.IsConnected;
        }

        public void ChangeCamera()
        {
            BurtCamera.SetActive(false);
            CliveCamera.SetActive(false);
            LuciusCamera.SetActive(false);

            switch (CameraDropdown.value)
            {
                case 0: BurtCamera.SetActive(true); break;
                case 1: CliveCamera.SetActive(true); break;
                case 2: LuciusCamera.SetActive(true); break;
            }
        }

        public void AttachAll()
        {
            foreach (var puppet in FindObjectsOfType<ButtPuppet>())
            {
                puppet.GrabAttachPose();
            }
        }
    }
}