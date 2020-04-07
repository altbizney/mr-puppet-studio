using System;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MrPuppet
{
    [Serializable]
    public class Pose
    {
        public Quaternion ShoulderRotation = Quaternion.identity;
        public Quaternion ElbowRotation = Quaternion.identity;
        public Quaternion WristRotation = Quaternion.identity;

        public void Set(Quaternion shoulder, Quaternion elbow, Quaternion wrist)
        {
            ShoulderRotation = shoulder;
            ElbowRotation = elbow;
            WristRotation = wrist;
        }

        public override string ToString()
        {
            return WristRotation.x + "," + WristRotation.y + "," + WristRotation.z + "," + WristRotation.w + ";" +
                ElbowRotation.x + "," + ElbowRotation.y + "," + ElbowRotation.z + "," + ElbowRotation.w + ";" +
                ShoulderRotation.x + "," + ShoulderRotation.y + "," + ShoulderRotation.z + "," + ShoulderRotation.w;
        }

        public void FromString(string[] wrist, string[] elbow, string[] shoulder)
        {
            WristRotation = new Quaternion(float.Parse(wrist[0]), float.Parse(wrist[1]), float.Parse(wrist[2]), float.Parse(wrist[3]));
            ElbowRotation = new Quaternion(float.Parse(elbow[0]), float.Parse(elbow[1]), float.Parse(elbow[2]), float.Parse(elbow[3]));
            ShoulderRotation = new Quaternion(float.Parse(shoulder[0]), float.Parse(shoulder[1]), float.Parse(shoulder[2]), float.Parse(shoulder[3]));
        }
    }

    public class MrPuppetDataMapper : MonoBehaviour
    {
        private MrPuppetHubConnection HubConnection;

        // yanked from Framer https://github.com/framer/Framer-fork/blob/master/framer/Utils.coffee#L285
        public float JawPercent => 0f + (((HubConnection.Jaw - JawClosed) / (float)(JawOpened - JawClosed)) * (1f - 0f));

        public enum Joint { Shoulder, Elbow, Wrist }

        public Transform ShoulderJoint { get; private set; }
        public Transform ElbowJoint { get; private set; }
        public Transform ElbowAnchorJoint { get; private set; }
        public Transform WristJoint { get; private set; }

        public bool EnableGizmo = true;

        [DisableInPlayMode()]
        public bool ShowJointChain = false;

        public Pose TPose;

        [Range(0f, 1023f)]
        public float JawOpened = 1023f;
        [Range(0f, 1023f)]
        public float JawClosed = 0f;

        [Range(.2f, 4f)]
        public float ArmLength = 1f;
        [Range(.2f, 4f)]
        public float ForearmLength = 1f;

        [MinValue(0f)]
        public float ForearmAnchorOffset = 0f;

        private void Awake()
        {
            HubConnection = FindObjectOfType<MrPuppetHubConnection>();

            var JointChain = new GameObject("• MrPuppet / Joint Chain").transform;
            JointChain.SetAsFirstSibling();
            JointChain.hideFlags = ShowJointChain ? HideFlags.None : HideFlags.HideInHierarchy;

            ShoulderJoint = new GameObject("Shoulder").transform;
            ShoulderJoint.SetParent(JointChain);

            ElbowJoint = new GameObject("Elbow").transform;
            ElbowJoint.SetParent(ShoulderJoint);

            ElbowAnchorJoint = new GameObject("Elbow - Anchor").transform;
            ElbowAnchorJoint.SetParent(ElbowJoint);

            WristJoint = new GameObject("Wrist").transform;
            WristJoint.SetParent(ElbowJoint);
        }

        private void Update()
        {
            // apply rotations *onto* TPose
            ShoulderJoint.rotation = TPose.ShoulderRotation * HubConnection.ShoulderRotation;

            ElbowJoint.localPosition = Vector3.back * ArmLength;
            ElbowJoint.rotation = TPose.ElbowRotation * HubConnection.ElbowRotation;

            ElbowAnchorJoint.localPosition = Vector3.back * ForearmAnchorOffset;

            WristJoint.localPosition = Vector3.back * ForearmLength;
            WristJoint.rotation = TPose.WristRotation * HubConnection.WristRotation;

            if (Input.GetKeyDown(KeyCode.T))
            {
                GrabTPose();
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                GrabJawOpened();
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                GrabJawClosed();
            }
        }

        public Transform GetJoint(Joint joint)
        {
            switch (joint)
            {
                case Joint.Shoulder:
                    return ShoulderJoint;
                case Joint.Elbow:
                    return ElbowJoint;
                case Joint.Wrist:
                    return WristJoint;
            }

            throw new ArgumentException("Invalid Joint");
        }

        [Button(ButtonSizes.Large)]
        [HorizontalGroup("TPose")]
        [GUIColor(0f, 1f, 0f)]
        [DisableInEditorMode]
        public void GrabTPose()
        {
            // store as the inverse, to save calculating it each frame
            TPose = new Pose
            {
                ShoulderRotation = Quaternion.Inverse(HubConnection.ShoulderRotation),
                ElbowRotation = Quaternion.Inverse(HubConnection.ElbowRotation),
                WristRotation = Quaternion.Inverse(HubConnection.WristRotation)
            };
            HubConnection.SendSocketMessage("COMMAND;TPOSE;" + TPose.ToString());
        }

        [Button(ButtonSizes.Large, Name = "Clear")]
        [HorizontalGroup("TPose", Width = .1f)]
        public void ClearTPose()
        {
            TPose = new Pose();
            HubConnection.SendSocketMessage("COMMAND;TPOSE;" + TPose.ToString());
        }

        [Button(ButtonSizes.Large)]
        [HorizontalGroup("Jaw")]
        [GUIColor(0f, 1f, 0f)]
        [DisableInEditorMode]
        public void GrabJawOpened()
        {
            JawOpened = HubConnection.Jaw;
            HubConnection.SendSocketMessage("COMMAND;JAW_OPENED;" + JawOpened);
        }

        [Button(ButtonSizes.Large)]
        [HorizontalGroup("Jaw")]
        [GUIColor(0f, 1f, 0f)]
        [DisableInEditorMode]
        public void GrabJawClosed()
        {
            JawClosed = HubConnection.Jaw;
            HubConnection.SendSocketMessage("COMMAND;JAW_CLOSED;" + JawClosed);
        }

        [Button(ButtonSizes.Large, Name = "Clear")]
        [HorizontalGroup("Jaw", Width = .1f)]
        public void ClearJaw()
        {
            JawClosed = 0f;
            JawOpened = 1023f;
            HubConnection.SendSocketMessage("COMMAND;JAW_OPENED;0");
            HubConnection.SendSocketMessage("COMMAND;JAW_CLOSED;1023");
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !EnableGizmo) return;

            // body
            Gizmos.color = Color.grey;
            Gizmos.DrawWireCube(new Vector3(0f, -0.5f, 0.5f), new Vector3(0.5f, 1.5f, 1f));
            Gizmos.DrawWireCube(new Vector3(0f, 0.4f, 0.5f), Vector3.one * 0.33f);

            Gizmos.color = Color.white;

            // shoulder
            Gizmos.matrix = ShoulderJoint.localToWorldMatrix;
            Gizmos.DrawWireCube(new Vector3(0f, 0f, ArmLength * -0.5f), new Vector3(0.25f, 0.25f, ArmLength));

            // eblow
            Gizmos.matrix = ElbowJoint.localToWorldMatrix;
            Gizmos.DrawWireCube(new Vector3(0f, 0f, ForearmLength * -0.5f), new Vector3(0.25f, 0.25f, ForearmLength));

            if (ForearmAnchorOffset > 0f) {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(new Vector3(0f, 0f, -ForearmAnchorOffset), 0.1f);
                Gizmos.color = Color.white;
            }

            // jaw
            Gizmos.matrix = Matrix4x4.TRS(WristJoint.position, WristJoint.rotation * Quaternion.Euler(Mathf.Lerp(0f, 45f, JawPercent) * 0.5f, 0f, 0f), Vector3.one);
            Gizmos.DrawWireCube(new Vector3(0f, 0.0625f, -0.25f), new Vector3(0.25f, 0.125f, 0.5f));
            Gizmos.matrix = Matrix4x4.TRS(WristJoint.position, WristJoint.rotation * Quaternion.Euler(Mathf.Lerp(0f, 45f, JawPercent) * -0.5f, 0f, 0f), Vector3.one);
            Gizmos.DrawWireCube(new Vector3(0f, -0.0625f, -0.25f), new Vector3(0.25f, 0.125f, 0.5f));

            Gizmos.matrix = Matrix4x4.identity;

            // axises
            Gizmos.color = Color.green;
            Gizmos.DrawRay(ShoulderJoint.position, ShoulderJoint.up * 0.25f);
            Gizmos.DrawRay(ElbowJoint.position, ElbowJoint.up * 0.25f);
            Gizmos.DrawRay(WristJoint.position, WristJoint.up * 0.25f);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(ShoulderJoint.position, ShoulderJoint.right * 0.25f);
            Gizmos.DrawRay(ElbowJoint.position, ElbowJoint.right * 0.25f);
            Gizmos.DrawRay(WristJoint.position, WristJoint.right * 0.25f);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(ShoulderJoint.position, ShoulderJoint.forward * 0.25f);
            Gizmos.DrawRay(ElbowJoint.position, ElbowJoint.forward * 0.25f);
            Gizmos.DrawRay(WristJoint.position, WristJoint.forward * 0.25f);
        }

        // The section below is used to store the changes made at runtime
        static MrPuppetDataMapper()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private const string TPoseShoulderRotationKey = "shoulderRotationTPose";
        private const string TPoseElbowRotationKey = "elbowRotationTPose";
        private const string TPoseWristRotationKey = "wristRotationTPose";
        private const string JawClosedKey = "jawClosed";
        private const string JawOpenedKey = "jawOpened";
        private const string ArmLengthKey = "armLength";
        private const string ForearmLengthKey = "forearmLength";

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            var dataMapper = FindObjectOfType<MrPuppetDataMapper>();
            if (dataMapper == null) return;

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                Undo.RecordObject(dataMapper, "Undo DataMapper");
                dataMapper.TPose = new Pose()
                {
                    ShoulderRotation = PlayerPrefsX.GetQuaternion(TPoseShoulderRotationKey),
                    ElbowRotation = PlayerPrefsX.GetQuaternion(TPoseElbowRotationKey),
                    WristRotation = PlayerPrefsX.GetQuaternion(TPoseWristRotationKey)
                };

                dataMapper.JawOpened = PlayerPrefs.GetFloat(JawOpenedKey);
                dataMapper.JawClosed = PlayerPrefs.GetFloat(JawClosedKey);

                dataMapper.ArmLength = PlayerPrefs.GetFloat(ArmLengthKey);
                dataMapper.ForearmLength = PlayerPrefs.GetFloat(ForearmLengthKey);
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                PlayerPrefsX.SetQuaternion(TPoseShoulderRotationKey, dataMapper.TPose.ShoulderRotation);
                PlayerPrefsX.SetQuaternion(TPoseElbowRotationKey, dataMapper.TPose.ElbowRotation);
                PlayerPrefsX.SetQuaternion(TPoseWristRotationKey, dataMapper.TPose.WristRotation);

                PlayerPrefs.SetFloat(JawOpenedKey, dataMapper.JawOpened);
                PlayerPrefs.SetFloat(JawClosedKey, dataMapper.JawClosed);

                PlayerPrefs.SetFloat(ArmLengthKey, dataMapper.ArmLength);
                PlayerPrefs.SetFloat(ForearmLengthKey, dataMapper.ForearmLength);
            }
        }
#endif
    }
}
