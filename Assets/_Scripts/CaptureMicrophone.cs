using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet
{
    [RequireComponent(typeof(AudioSource))]
    public class CaptureMicrophone : MonoBehaviour
    {
        private ValueDropdownList<string> DeviceNames = new ValueDropdownList<string>();

        [ValueDropdown("DeviceNames")]
        [OnValueChanged(nameof(OnDeviceNameChanged))]
        public string DeviceName;

        public int Frequency;

        [ShowInInspector, ReadOnly]
        private int minFreq;

        [ShowInInspector, ReadOnly]
        private int maxFreq;

        private AudioSource AudioSource;

        private void OnDeviceNameChanged()
        {
            Microphone.GetDeviceCaps(DeviceName, out minFreq, out maxFreq);

            // if (Frequency == 0) Frequency = minFreq;
            // if (Frequency > maxFreq) Frequency = maxFreq;
            // if (Frequency < minFreq) Frequency = minFreq;
        }

        private void OnValidate()
        {
            DeviceNames.Clear();
            foreach (var name in Microphone.devices)
            {
                DeviceNames.Add(name);

                if (DeviceName == null)
                {
                    DeviceName = name;
                }
            }
        }

        private void Start()
        {
            AudioSource = GetComponent<AudioSource>();
            AudioSource.playOnAwake = false;
            AudioSource.loop = true;
            AudioSource.spatialBlend = 0f;
            AudioSource.clip = Microphone.Start(DeviceName, true, 1, Frequency);
            while (!(Microphone.GetPosition(DeviceName) > 0)) { }
            AudioSource.Play();
        }
    }
}