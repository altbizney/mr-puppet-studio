using System;
using UnityEngine;

namespace Thinko
{
    public abstract class RealPuppetDataProvider : MonoBehaviour
    {
        public class SensorCalibrationData
        {
            public int System;
            public int Gyro;
            public int Accelerometer;
            public int Magnetometer;

            public void Set(int system, int gyro, int accelerometer, int magnetometer)
            {
                System = system;
                Gyro = gyro;
                Accelerometer = accelerometer;
                Magnetometer = magnetometer;
            }

            public bool IsCalibrated => System == 3 && Gyro == 3 && Accelerometer == 3 && Magnetometer == 3;
        }

        public enum Source
        {
            Wrist,
            Elbow,
            Shoulder
        }

        public Quaternion GetInput(Source source)
        {
            var q = Quaternion.identity;
            switch (source)
            {
                case Source.Wrist:
                    q = WristRotation;
                    break;
                case Source.Elbow:
                    q = ElbowRotation;
                    break;
                case Source.Shoulder:
                    q = ShoulderRotation;
                    break;
            }

            return q;
        }

        public SensorCalibrationData GetSensorCalibrationData(Source source)
        {
            switch (source)
            {
                case Source.Wrist:
                    return WristCalibrationData;
                case Source.Elbow:
                    return ElbowCalibrationData;
                case Source.Shoulder:
                    return ShoulderCalibrationData;
            }
            return null;
        }

        [ReadOnly]
        public Quaternion WristRotation;

        public SensorCalibrationData WristCalibrationData;

        [ReadOnly]
        public Quaternion ElbowRotation;

        public SensorCalibrationData ElbowCalibrationData;

        [ReadOnly]
        public Quaternion ShoulderRotation;

        public SensorCalibrationData ShoulderCalibrationData;

        [ReadOnly]
        public float Jaw;

        private void Awake()
        {
            WristCalibrationData = new SensorCalibrationData();
            ElbowCalibrationData = new SensorCalibrationData();
            ShoulderCalibrationData = new SensorCalibrationData();
        }
    }
}