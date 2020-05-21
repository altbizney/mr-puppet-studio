using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor.Animations;


namespace MrPuppet
{
    public class ButtPuppet : MonoBehaviour
    {
        [Serializable]
        public class WeightedInfluence
        {
            public MrPuppetDataMapper.Joint joint;
            public Transform target;
            // private Transform proxy;

            [Range(0f, 1f)]
            public float amount = 1f;

            private Quaternion spawn;
            private Quaternion full;
            private Quaternion weighted;
            private Quaternion attach;


            public void SnapshotSpawn()
            {
                // proxy = new GameObject("Proxy:" + target.name).transform;
                // proxy.SetPositionAndRotation(target.position, target.rotation);
                // proxy.SetParent(target.parent, false);

                spawn = target.rotation;
            }

            private Quaternion Attach(MrPuppetDataMapper DataMapper)
            {
                switch (joint)
                {
                    case MrPuppetDataMapper.Joint.Elbow:
                        return DataMapper.AttachPose.ElbowRotation;
                    case MrPuppetDataMapper.Joint.Wrist:
                        return DataMapper.AttachPose.WristRotation;
                    default:
                        return Quaternion.identity;
                }
            }

            public void Update(MrPuppetDataMapper DataMapper, float RotationSpeed, float SensorAmount)
            {
                if (!target) return;

                // calculate fully blended extent
                full = (DataMapper.GetJoint(joint).rotation * Quaternion.Inverse(Attach(DataMapper))) * spawn;

                // calculate weighted rotation
                weighted = Quaternion.Slerp(spawn, full, amount * SensorAmount);

                // calculate deattach rotation
                //attach = Quaternion.Slerp(weighted, spawn, AttachAmount);

                // apply with smoothing
                target.rotation = Quaternion.Slerp(target.rotation, attach, RotationSpeed * Time.deltaTime);
            }

            public void OnDrawGizmos()
            {
                if (!target) return;

                Debug.DrawRay(target.position, target.up * 0.5f, Color.green, 0f, false);
                Debug.DrawRay(target.position, target.right * 0.5f, Color.red, 0f, false);
                Debug.DrawRay(target.position, target.forward * 0.5f, Color.blue, 0f, false);
            }
        }

        private MrPuppetDataMapper DataMapper;
        private MrPuppetHubConnection HubConnection;

        // spawn position of proxy geo
        private Vector3 HipSpawnPosition;
        private Quaternion HipSpawnRotation;
        private Quaternion HeadSpawnRotation;

        public Transform Hip;
        public Transform Head;
        // private Transform HipProxy;
        // private Transform HeadProxy;

        private Quaternion UnsubscribeHipRotation;
        private Quaternion UnsubscribeHeadRotation;
        private bool Unsubscribed = true;
        private bool UnsubscribeForward;
        private string UnsubscribeButtonLabel = "Hardware control disabled. Attach to enable";
        private float LerpTimer;
        private List<JawBlendShapeMapper> JawBlendshapeComponents = new List<JawBlendShapeMapper>();
        private List<JawTransformMapper> JawTransformComponents = new List<JawTransformMapper>();

        private Vector3 position;
        private GameObject PuppetIdle;
        private Transform IdleHip;
        private Transform IdleHead;

        private List<Transform> JointsMimic = new List<Transform>();
        private List<Transform> JointsClone = new List<Transform>();
        private List<Vector3> JointsSpawnPosition = new List<Vector3>();
        private List<Quaternion> JointsSpawnRotation = new List<Quaternion>();

        public List<WeightedInfluence> WeightedInfluences = new List<WeightedInfluence>();

        [MinValue(0f)]
        public float RotationSpeed = 7f;
        [MinValue(0f)]
        public float PositionSpeed = 0.1f;
        private Vector3 PositionVelocity;

        [HorizontalGroup("HipExtentX")]
        public bool LimitHipExtentX = false;
        [HorizontalGroup("HipExtentX")]
        [ShowIf("LimitHipExtentX")]
        public float HipExtentX = 0f;

        [HorizontalGroup("HipExtentY")]
        public bool LimitHipExtentY = false;
        [HorizontalGroup("HipExtentY")]
        [ShowIf("LimitHipExtentY")]
        public float HipExtentY = 0f;

        [HorizontalGroup("HipExtentZ")]
        public bool LimitHipExtentZ = false;
        [HorizontalGroup("HipExtentZ")]
        [ShowIf("LimitHipExtentZ")]
        public float HipExtentZ = 0f;

        public bool EnableJawHeadMixer = false;
        [ShowIf("EnableJawHeadMixer")]
        public float JawHeadMaxExtent = 10f;

        public enum JawHeadAxis { x, y, z }

        [ShowIf("EnableJawHeadMixer")]
        [EnumToggleButtons]
        public JawHeadAxis JawHeadRotate = JawHeadAxis.z;

        [HideInInspector]
        public bool ApplySensors = true;

        [MinValue(0.01f)]
        [TitleGroup("Sensor Subscription")]
        [OnValueChanged("ChangedDuration")]
        public float UnsubscribeDuration = 1f;

        [ReadOnly]
        [Range(0f, 1f)]
        [TitleGroup("Sensor Subscription")]
        public float SensorAmount = 0f;

        [Button(ButtonSizes.Large)]
        [GUIColor(0f, 1f, 0f)]
        [DisableInEditorMode()]
        [TitleGroup("Sensor Subscription")]
        [LabelText("$UnsubscribeButtonLabel")]
        [DisableIf("$Unsubscribed")]
        public void UnsubscribeFromSensors()
        {
            if (!Unsubscribed)
            {
                LerpTimer = UnsubscribeDuration;
                Unsubscribed = true;
                SensorAmount = 0;
                UnsubscribeForward = false;
                UnsubscribeButtonLabel = "Hardware control disabled. Re-attach to enable";
            }
        }

        public void SubscribeEventButtPuppet()
        {
            if (Unsubscribed)
            {
                UnsubscribeForward = true;
            }
        }

        private void ChangedDuration()
        {
            if (!Unsubscribed)
            {
                LerpTimer = UnsubscribeDuration;
                SensorAmount = 1f;
            }
        }

        private void Awake()
        {
            DataMapper = FindObjectOfType<MrPuppetDataMapper>();
            HubConnection = FindObjectOfType<MrPuppetHubConnection>();

            ApplySensors = true;

            // // clone proxy geo
            // HipProxy = new GameObject("Proxy:" + Hip.name).transform;
            // HipProxy.SetPositionAndRotation(Hip.position, Hip.rotation);
            // HipProxy.SetParent(Hip.parent, false);

            // HeadProxy = new GameObject("Proxy:" + Head.name).transform;
            // HeadProxy.SetPositionAndRotation(Head.position, Head.rotation);
            // HeadProxy.SetParent(Head.parent, false);

            HipSpawnPosition = Hip.localPosition;
            HipSpawnRotation = Hip.rotation;
            HeadSpawnRotation = Head.rotation;

            // snapshot bind poses of weighted influence targets
            foreach (var influence in WeightedInfluences)
            {
                influence.SnapshotSpawn();
            }

            DataMapper.OnSubscribeEvent += SubscribeEventButtPuppet;

            foreach (JawTransformMapper jaw in gameObject.GetComponentsInChildren<JawTransformMapper>())
            {
                JawTransformComponents.Add(jaw);
            }
            foreach (JawBlendShapeMapper jaw in gameObject.GetComponentsInChildren<JawBlendShapeMapper>())
            {
                JawBlendshapeComponents.Add(jaw);
            }

            Animator _Animator = gameObject.GetComponentInChildren<Animator>();

            PuppetIdle = Instantiate(_Animator.gameObject, gameObject.transform.position + new Vector3(0, 0, 3f), gameObject.transform.localRotation);
            PuppetIdle.transform.Rotate(0, 90f, 0);
            foreach (Transform child in PuppetIdle.GetComponentsInChildren<Transform>(true)) { child.gameObject.layer = 8; }

            _Animator.enabled = false;

            foreach (Transform child in PuppetIdle.transform.GetComponentsInChildren<Transform>())
            {
                foreach (Transform nestedChild in gameObject.transform.GetComponentsInChildren<Transform>())
                {
                    if (nestedChild.name == child.name)
                    {
                        if (Hip.name == child.name)
                            IdleHip = child;
                        else if (Head.name == child.name)
                            IdleHead = child;
                        else
                        {
                            JointsMimic.Add(nestedChild);
                            JointsClone.Add(child);

                            JointsSpawnRotation.Add(nestedChild.localRotation);
                            JointsSpawnPosition.Add(nestedChild.localPosition);
                        }
                    }
                }
            }
        }

        private void Update()
        {
            // apply position delta to bind pose
            if (ApplySensors == true)
            {
                position = HipSpawnPosition + (DataMapper.ElbowAnchorJoint.position - DataMapper.AttachPose.ElbowPosition);

                if (Unsubscribed)
                {
                    if (UnsubscribeForward)
                        LerpTimer += Time.deltaTime;
                    else
                        LerpTimer -= Time.deltaTime;

                    SensorAmount = LerpTimer / UnsubscribeDuration;
                    SensorAmount = SensorAmount * SensorAmount * (3f - 2f * SensorAmount);
                }

                if (LerpTimer > UnsubscribeDuration && UnsubscribeForward)
                {
                    LerpTimer = UnsubscribeDuration;
                    UnsubscribeButtonLabel = "Disable hardware control";
                    Unsubscribed = false;
                    SensorAmount = 1;
                }
                else if (LerpTimer < 0 && !UnsubscribeForward)
                {
                    LerpTimer = 0;
                    SensorAmount = 0;
                }

                for (var i = 0; i < JointsMimic.Count; i++)
                {
                    JointsMimic[i].localRotation = Quaternion.Slerp(JointsClone[i].localRotation, JointsSpawnRotation[i], SensorAmount);
                    JointsMimic[i].localPosition = Vector3.Lerp(JointsClone[i].localPosition, JointsSpawnPosition[i], SensorAmount);
                }

                position = Vector3.Lerp(IdleHip.localPosition, position, SensorAmount);
                UnsubscribeHipRotation = Quaternion.Slerp(IdleHip.rotation, (DataMapper.ElbowJoint.rotation * Quaternion.Inverse(DataMapper.AttachPose.ElbowRotation)) * HipSpawnRotation, SensorAmount);
                UnsubscribeHeadRotation = Quaternion.Slerp(IdleHead.rotation, (DataMapper.WristJoint.rotation * Quaternion.Inverse(DataMapper.AttachPose.WristRotation)) * HeadSpawnRotation, SensorAmount);

                foreach (JawBlendShapeMapper Jaw in JawBlendshapeComponents) { Jaw.SensorAmount = SensorAmount; }
                foreach (JawTransformMapper Jaw in JawTransformComponents) { Jaw.SensorAmount = SensorAmount; }

                // clamp to XYZ extents (BEFORE smooth)
                position.Set(
                    LimitHipExtentX ? Mathf.Clamp(position.x, HipSpawnPosition.x - HipExtentX, HipSpawnPosition.x + HipExtentX) : position.x,
                    LimitHipExtentY ? Mathf.Clamp(position.y, HipSpawnPosition.y - HipExtentY, HipSpawnPosition.y + HipExtentY) : position.y,
                    LimitHipExtentZ ? Mathf.Clamp(position.z, HipSpawnPosition.z - HipExtentZ, HipSpawnPosition.z + HipExtentZ) : position.z
                );

                // smoothly apply changes to position
                Hip.localPosition = Vector3.SmoothDamp(Hip.localPosition, position, ref PositionVelocity, PositionSpeed);

                // apply rotation deltas to bind pose
                Hip.rotation = Quaternion.Slerp(Hip.rotation, UnsubscribeHipRotation, RotationSpeed * Time.deltaTime);
                Head.rotation = Quaternion.Slerp(Head.rotation, UnsubscribeHeadRotation, RotationSpeed * Time.deltaTime);

                if (EnableJawHeadMixer)
                {
                    switch (JawHeadRotate)
                    {
                        case JawHeadAxis.x:
                            Head.Rotate(Mathf.Lerp(0f, JawHeadMaxExtent, DataMapper.JawPercent * SensorAmount), 0f, 0f, Space.Self);
                            break;

                        case JawHeadAxis.y:
                            Head.Rotate(0f, Mathf.Lerp(0f, JawHeadMaxExtent, DataMapper.JawPercent * SensorAmount), 0f, Space.Self);
                            break;

                        case JawHeadAxis.z:
                            Head.Rotate(0f, 0f, Mathf.Lerp(0f, JawHeadMaxExtent, DataMapper.JawPercent * SensorAmount), Space.Self);
                            break;
                    }
                }

                // apply weighted influences
                foreach (var influence in WeightedInfluences)
                {
                    influence.Update(DataMapper, RotationSpeed, SensorAmount);
                }

                if (Input.GetKeyDown(KeyCode.D)) { UnsubscribeFromSensors(); }
            }
        }


        // REMINDER: Change = Quaternion.Inverse(Last) * Current;

        // private void LateUpdate()
        // {
        //     if (!(HipProxy && HeadProxy)) return;

        //     Hip.position = Hip.position - (HipProxySpawnPosition - HipProxy.position);

        //     Hip.rotation = Hip.rotation * (Quaternion.Inverse(Hip.rotation) * HipProxy.rotation);
        //     Head.rotation = Head.rotation * (Quaternion.Inverse(Head.rotation) * HeadProxy.rotation);

        //     // WIP: this basically is world-relative. also sort of busted on head....
        //     // Hip.rotation = Hip.rotation * (HipProxy.rotation * Quaternion.Inverse(HipProxySpawnRotation));
        //     // Head.rotation = Head.rotation * (HeadProxy.localRotation * Quaternion.Inverse(HeadProxySpawnRotation));
        // }

        private void OnDrawGizmos()
        {
            if (Hip) Debug.DrawRay(Hip.position, Hip.up * 0.5f, Color.green, 0f, false);
            if (Head) Debug.DrawRay(Head.position, Head.up * 0.5f, Color.green, 0f, false);

            if (Hip) Debug.DrawRay(Hip.position, Hip.right * 0.5f, Color.red, 0f, false);
            if (Head) Debug.DrawRay(Head.position, Head.right * 0.5f, Color.red, 0f, false);

            if (Hip) Debug.DrawRay(Hip.position, Hip.forward * 0.5f, Color.blue, 0f, false);
            if (Head) Debug.DrawRay(Head.position, Head.forward * 0.5f, Color.blue, 0f, false);

            foreach (var influence in WeightedInfluences) influence.OnDrawGizmos();

            if (LimitHipExtentX)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(Application.isPlaying ? HipSpawnPosition : transform.position, new Vector3(HipExtentX * 2f, 0.001f, LimitHipExtentZ ? HipExtentZ * 2f : 0.1f));
            }

            if (LimitHipExtentY)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(Application.isPlaying ? HipSpawnPosition : transform.position, new Vector3(LimitHipExtentX ? HipExtentX * 2f : 0.1f, HipExtentY * 2f, 0.001f));
            }

            if (LimitHipExtentZ)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(Application.isPlaying ? HipSpawnPosition : transform.position, new Vector3(0.001f, LimitHipExtentY ? HipExtentY * 2f : 0.1f, HipExtentZ * 2f));
            }
        }
    }
}
