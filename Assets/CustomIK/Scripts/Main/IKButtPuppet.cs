using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using RootMotion.FinalIK;

namespace MrPuppet
{
    public class IKButtPuppet : MonoBehaviour
    {

        #region Static Variables

        #endregion

        #region Public Variables
        #region Legacy
        [Title("Legacy")]
        public Transform HipRotation;
        public Transform HipTranslation;
        public Transform Head;

        public List<ButtPuppet.WeightedInfluence> WeightedInfluences = new List<ButtPuppet.WeightedInfluence>();

        [MinValue(0f)]
        public float RotationSpeed = 7f;
        [MinValue(0f)]
        public float PositionSpeed = 0.1f;

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
        #endregion

        #region SensorSubscription
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
                SetInitialIKWeights();
                LerpTimer = UnsubscribeDuration;
                Unsubscribed = true;
                SensorAmount = 0;
                UnsubscribeForward = false;
                UnsubscribeButtonLabel = "Hardware control disabled. Re-attach to enable";
            }
        }

        public void SubscribeEventIKButtPuppet()
        {
            if (Unsubscribed)
            {
                UnsubscribeForward = true;
            }
        }
        #endregion


        #region IK
        [DetailedInfoBox("IK bones help", "For the Bones 1-3 in each IK component (minus the 'Grounder') they will be oriented from tip to root instead of root to tip and example is as follows - \n\n Example Rig: UpperArm => Forearm => Wrist \n UpperArm: Bone 3 \n Forearm: Bone 2 \n Wrist: Bone 1 \n\n **The exception to this rule is the legs")]
        [DetailedInfoBox("IK tags help", "To assign tags to bones and IK objects simply add the IKTag component to the needed objects \n\n ikTagId: The bone reference \n chainId: The order of the bone relative to the bone reference")]
        [TitleGroup("IK", "These components will follow tracking data from the data transport")]
        [Required]
        public Transform rigNode;
        [Required]
        public Transform[] ikNodes;

        [TabGroup("Spine Stack", "Hip")]
        [Required]
        public TrigonometricIK hipIK;
        [TabGroup("Spine Stack", "Hip")]
        [Range(0, 1)]
        public float hipIKPositionWeight;
        [TabGroup("Spine Stack", "Hip")]
        [Range(0, 1)]
        public float hipIKRotationWeight;
        [TabGroup("Spine Stack", "Hip")]
        [Required]
        public Transform hipIKTarget;

        [TabGroup("Spine Stack", "Spine")]
        [Required]
        public TrigonometricIK spineIK;
        [TabGroup("Spine Stack", "Spine")]
        [Range(0, 1)]
        public float spineIKPositionWeight;
        [TabGroup("Spine Stack", "Spine")]
        [Range(0, 1)]
        public float spineIKRotationWeight;
        [TabGroup("Spine Stack", "Spine")]
        [Required]
        public Transform spineIKTarget;

        [TabGroup("Spine Stack", "Neck")]
        [Required]
        public TrigonometricIK neckIK;
        [TabGroup("Spine Stack", "Neck")]
        [Range(0, 1)]
        public float neckIKPositionWeight;
        [TabGroup("Spine Stack", "Neck")]
        [Range(0, 1)]
        public float neckIKRotationWeight;
        [TabGroup("Spine Stack", "Neck")]
        [Required]
        public Transform neckIKTarget;

        [TabGroup("Spine Stack", "Head")]
        [Required]
        public TrigonometricIK headIK;
        [TabGroup("Spine Stack", "Head")]
        [Range(0, 1)]
        public float headIKPositionWeight;
        [TabGroup("Spine Stack", "Head")]
        [Range(0, 1)]
        public float headIKRotationWeight;
        [TabGroup("Spine Stack", "Head")]
        [Required]
        public Transform headIKTarget;

        [TabGroup("Arm Stack", "Left Arm")]
        public bool enableLeftArmLimb;
        [ShowIf("enableLeftArmLimb")]
        [TabGroup("Arm Stack", "Left Arm")]
        [Required]
        public LimbIK leftArmLimb;
        [ShowIf("enableLeftArmLimb")]
        [TabGroup("Arm Stack", "Left Arm")]
        [Range(0, 1)]
        public float leftArmLimbPositionWeight;
        [ShowIf("enableLeftArmLimb")]
        [TabGroup("Arm Stack", "Left Arm")]
        [Range(0, 1)]
        public float leftArmLimbRotationWeight;
        [ShowIf("enableLeftArmLimb")]
        [TabGroup("Arm Stack", "Left Arm")]
        [Required]
        public Transform leftArmTarget;

        [TabGroup("Arm Stack", "Right Arm")]
        public bool enableRightArmLimb;
        [ShowIf("enableRightArmLimb")]
        [TabGroup("Arm Stack", "Right Arm")]
        [Required]
        public LimbIK rightArmLimb;
        [ShowIf("enableRightArmLimb")]
        [TabGroup("Arm Stack", "Right Arm")]
        [Range(0, 1)]
        public float rightArmLimbPositionWeight;
        [ShowIf("enableRightArmLimb")]
        [TabGroup("Arm Stack", "Right Arm")]
        [Range(0, 1)]
        public float rightArmLimbRotationWeight;
        [ShowIf("enableRightArmLimb")]
        [TabGroup("Arm Stack", "Right Arm")]
        [Required]
        public Transform rightArmTarget;

        [DetailedInfoBox("Grounder legs help", "For the legs, simply drag and drop the left and right LimbIK objects into the array field \n\n **For humanoids should be no more than 2 objects in the legs array on the grounder")]
        [TabGroup("Grounder Stack", "Grounder")]
        [Required]
        public GrounderIK grounderIK;
        [TabGroup("Grounder Stack", "Grounder")]
        [Range(0, 1)]
        public float grounderIKWeight;
        [TabGroup("Grounder Stack", "Left Leg")]
        [Required]
        public LimbIK leftLegLimb;
        [TabGroup("Grounder Stack", "Left Leg")]
        [Range(0, 1)]
        public float leftLegLimbRotationWeight;
        [TabGroup("Grounder Stack", "Right Leg")]
        [Required]
        public LimbIK rightLegLimb;
        [TabGroup("Grounder Stack", "Right Leg")]
        [Range(0, 1)]
        public float rightLegLimbRotationWeight;
        #endregion
        #endregion

        #region Private Variables
        private MrPuppetDataMapper DataMapper;
        private MrPuppetHubConnection HubConnection;

        // spawn position of proxy geo
        private Vector3 HipSpawnPosition;
        private Quaternion HipSpawnRotation;
        private Quaternion HeadSpawnRotation;

        private Vector3 position;
        private Vector3 PositionVelocity;

        [HideInInspector]
        public bool ApplySensors = true;

        private Quaternion UnsubscribeHeadRotation;
        private Quaternion UnsubscribeHipRotation;
        private bool Unsubscribed = true;
        private bool UnsubscribeForward;
        private string UnsubscribeButtonLabel = "Hardware control disabled. Attach to enable";
        private float LerpTimer;
        private List<JawBlendShapeMapper> JawBlendshapeComponents = new List<JawBlendShapeMapper>();
        private List<JawTransformMapper> JawTransformComponents = new List<JawTransformMapper>();

        private float InitialHipIKRotationWeight;
        private float InitialHipIKPositionWeight;
        private float InitialHeadIKPositionWeight;
        private float InitialHeadIKRotationWeight;
        private float InitialLeftArmLimbPositionWeight;
        private float InitialLeftArmLimbRotationWeight;
        private float InitialRightArmLimbPositionWeight;
        private float InitialRightArmLimbRotationWeight;
        private float InitialGrounderIKWeight;
        private float InitialLeftLegLimbRotationWeight;
        private float InitialRightLegLimbRotationWeight;
        private float InitialSpineIKRotationWeight;
        private float InitialSpineIKPositionWeight;
        private float InitialNeckIKPositionWeight;
        private float InitialNeckIKRotationWeight;

        private Animator _Animator;
        #endregion

        #region Unity Methods
        private void Awake()
        {
            LegacyAwake();
        }

        private void Update()
        {
            IKUpdate();
            LegacyUpdate();
        }
        #endregion

        #region Callback Methods

        #endregion

        #region Static Methods

        #endregion

        #region Public Methods

        #endregion

        #region Local Methods
        [Title("Misc")]
        [Button("Assign IK Nodes", 25, ButtonStyle.Box)]
        [GUIColor(1f, 1f, 0f)]
        [DisableInPlayMode()]
        private void AssignIKNodes()
        {
            List<IKTag> tags = new List<IKTag>();
            foreach (Transform t in ikNodes)
            {
                tags.AddRange(t.GetComponentsInChildren<IKTag>());
            }
            foreach (IKTag tag in tags)
            {
                if (tag.iKTagId == IKTagId.Hip)
                {
                    hipIK = tag.GetComponent<TrigonometricIK>();
                    if (hipIK && hipIK.solver.bone3.transform)
                    {
                        hipIK.transform.position = hipIK.solver.bone3.transform.position;
                        hipIK.transform.rotation = rigNode.rotation;
                    }
                }
                if (tag.iKTagId == IKTagId.Spine)
                {
                    spineIK = tag.GetComponent<TrigonometricIK>();
                    if (spineIK && spineIK.solver.bone3.transform)
                    {
                        spineIK.transform.position = spineIK.solver.bone3.transform.position;
                        spineIK.transform.rotation = rigNode.rotation;
                    }
                }
                if (tag.iKTagId == IKTagId.Neck)
                {
                    neckIK = tag.GetComponent<TrigonometricIK>();
                    if (neckIK && neckIK.solver.bone3.transform)
                    {
                        neckIK.transform.position = neckIK.solver.bone3.transform.position;
                        neckIK.transform.rotation = rigNode.rotation;
                    }
                }
                if (tag.iKTagId == IKTagId.Head)
                {
                    headIK = tag.GetComponent<TrigonometricIK>();
                    if (headIK && headIK.solver.bone3.transform)
                    {
                        headIK.transform.position = headIK.solver.bone3.transform.position;
                        headIK.transform.rotation = rigNode.rotation;
                    }
                }
                if (tag.iKTagId == IKTagId.LeftArm && enableLeftArmLimb)
                {
                    leftArmLimb = tag.GetComponent<LimbIK>();
                    if (leftArmLimb && leftArmLimb.solver.bone1.transform)
                    {
                        leftArmLimb.transform.position = leftArmLimb.solver.bone1.transform.position;
                        leftArmLimb.transform.rotation = rigNode.rotation;
                    }
                }
                if (tag.iKTagId == IKTagId.RightArm && enableRightArmLimb)
                {
                    rightArmLimb = tag.GetComponent<LimbIK>();
                    if (rightArmLimb && rightArmLimb.solver.bone1.transform)
                    {
                        rightArmLimb.transform.position = rightArmLimb.solver.bone1.transform.position;
                        rightArmLimb.transform.rotation = rigNode.rotation;
                    }
                }
                if (tag.iKTagId == IKTagId.Grounder)
                {
                    grounderIK = tag.GetComponent<GrounderIK>();
                }
                if (tag.iKTagId == IKTagId.LeftLeg)
                {
                    leftLegLimb = tag.GetComponent<LimbIK>();
                    if (leftLegLimb && leftLegLimb.solver.bone3.transform)
                    {
                        leftLegLimb.transform.position = leftLegLimb.solver.bone3.transform.position;
                        leftLegLimb.transform.rotation = rigNode.rotation;

                        if (leftLegLimb.solver.target)
                        {
                            leftLegLimb.solver.target.position = leftLegLimb.transform.position;
                            leftLegLimb.solver.target.rotation = leftLegLimb.transform.rotation;
                        }
                    }
                }
                if (tag.iKTagId == IKTagId.RightLeg)
                {
                    rightLegLimb = tag.GetComponent<LimbIK>();
                    if (rightLegLimb && rightLegLimb.solver.bone3.transform)
                    {
                        rightLegLimb.transform.position = rightLegLimb.solver.bone3.transform.position;
                        rightLegLimb.transform.rotation = rigNode.rotation;

                        if (rightLegLimb.solver.target)
                        {
                            rightLegLimb.solver.target.position = rightLegLimb.transform.position;
                            rightLegLimb.solver.target.rotation = rightLegLimb.transform.rotation;
                        }
                    }
                }
            }
        }

        [Button("Assign IK Bones", 25, ButtonStyle.Box)]
        [GUIColor(1f, 1f, 0f)]
        [DisableInPlayMode()]
        private void AssignIKBones()
        {
            IKTag[] tags = rigNode.GetComponentsInChildren<IKTag>();

            //Hip
            if (hipIK)
            {
                foreach (IKTag tag in tags)
                {
                    if (tag.iKTagId == IKTagId.Hip)
                    {
                        hipIK.solver.target = hipIKTarget;
                        switch (tag.chainId)
                        {
                            case 1:
                                hipIK.solver.bone1.transform = tag.transform;
                                break;
                            case 2:
                                hipIK.solver.bone2.transform = tag.transform;
                                break;
                            case 3:
                                hipIK.solver.bone3.transform = tag.transform;
                                break;
                        }
                    }
                }
            }

            //Spine
            if (spineIK)
            {
                foreach (IKTag tag in tags)
                {
                    if (tag.iKTagId == IKTagId.Spine)
                    {
                        spineIK.solver.target = spineIKTarget;
                        switch (tag.chainId)
                        {
                            case 1:
                                spineIK.solver.bone1.transform = tag.transform;
                                break;
                            case 2:
                                spineIK.solver.bone2.transform = tag.transform;
                                break;
                            case 3:
                                spineIK.solver.bone3.transform = tag.transform;
                                break;
                        }
                    }
                }
            }

            //Neck
            if (neckIK)
            {
                foreach (IKTag tag in tags)
                {
                    if (tag.iKTagId == IKTagId.Neck)
                    {
                        neckIK.solver.target = neckIKTarget;
                        switch (tag.chainId)
                        {
                            case 1:
                                neckIK.solver.bone1.transform = tag.transform;
                                break;
                            case 2:
                                neckIK.solver.bone2.transform = tag.transform;
                                break;
                            case 3:
                                neckIK.solver.bone3.transform = tag.transform;
                                break;
                        }
                    }
                }
            }

            //Head
            if (headIK)
            {
                foreach (IKTag tag in tags)
                {
                    if (tag.iKTagId == IKTagId.Head)
                    {
                        headIK.solver.target = headIKTarget;
                        switch (tag.chainId)
                        {
                            case 1:
                                headIK.solver.bone1.transform = tag.transform;
                                break;
                            case 2:
                                headIK.solver.bone2.transform = tag.transform;
                                break;
                            case 3:
                                headIK.solver.bone3.transform = tag.transform;
                                break;
                        }
                    }
                }
            }

            //Left Arm
            if (enableLeftArmLimb)
            {
                foreach (IKTag tag in tags)
                {
                    if (tag.iKTagId == IKTagId.LeftArm)
                    {
                        leftArmLimb.solver.target = leftArmTarget;
                        switch (tag.chainId)
                        {
                            case 1:
                                leftArmLimb.solver.bone1.transform = tag.transform;
                                break;
                            case 2:
                                leftArmLimb.solver.bone2.transform = tag.transform;
                                break;
                            case 3:
                                leftArmLimb.solver.bone3.transform = tag.transform;
                                break;
                        }
                    }
                }
            }

            //Right Arm
            if (enableRightArmLimb)
            {
                foreach (IKTag tag in tags)
                {
                    if (tag.iKTagId == IKTagId.RightArm)
                    {
                        rightArmLimb.solver.target = rightArmTarget;
                        switch (tag.chainId)
                        {
                            case 1:
                                rightArmLimb.solver.bone1.transform = tag.transform;
                                break;
                            case 2:
                                rightArmLimb.solver.bone2.transform = tag.transform;
                                break;
                            case 3:
                                rightArmLimb.solver.bone3.transform = tag.transform;
                                break;
                        }
                    }
                }
            }

            //Grounder
            if (grounderIK)
            {
                foreach (IKTag tag in tags)
                {
                    if (tag.iKTagId == IKTagId.Grounder)
                    {
                        switch (tag.chainId)
                        {
                            case 1:
                                grounderIK.pelvis = tag.transform;
                                break;
                            case 2:
                                grounderIK.characterRoot = tag.transform;
                                break;
                        }
                    }
                }
            }

            //Left Leg
            if (leftLegLimb)
            {
                foreach (IKTag tag in tags)
                {
                    if (tag.iKTagId == IKTagId.LeftLeg)
                    {
                        switch (tag.chainId)
                        {
                            case 1:
                                leftLegLimb.solver.bone1.transform = tag.transform;
                                break;
                            case 2:
                                leftLegLimb.solver.bone2.transform = tag.transform;
                                break;
                            case 3:
                                leftLegLimb.solver.bone3.transform = tag.transform;
                                break;
                        }
                    }
                }
            }

            //Right Leg
            if (rightLegLimb)
            {
                foreach (IKTag tag in tags)
                {
                    if (tag.iKTagId == IKTagId.RightLeg)
                    {
                        switch (tag.chainId)
                        {
                            case 1:
                                rightLegLimb.solver.bone1.transform = tag.transform;
                                break;
                            case 2:
                                rightLegLimb.solver.bone2.transform = tag.transform;
                                break;
                            case 3:
                                rightLegLimb.solver.bone3.transform = tag.transform;
                                break;
                        }
                    }
                }
            }
        }

        [Button("Update IK Weights", 25, ButtonStyle.Box)]
        [GUIColor(1f, 1f, 0f)]
        [DisableInPlayMode()]
        private void UpdateIKWeights()
        {
            //Hip
            if (hipIK)
            {
                hipIK.solver.IKPositionWeight = hipIKPositionWeight;
                hipIK.solver.IKRotationWeight = hipIKRotationWeight;
            }

            //Spine
            if (spineIK)
            {
                spineIK.solver.IKPositionWeight = spineIKPositionWeight;
                spineIK.solver.IKRotationWeight = spineIKRotationWeight;
            }

            //Neck
            if (neckIK)
            {
                neckIK.solver.IKPositionWeight = neckIKPositionWeight;
                neckIK.solver.IKRotationWeight = neckIKRotationWeight;
            }

            //Head
            if (headIK)
            {
                headIK.solver.IKPositionWeight = headIKPositionWeight;
                headIK.solver.IKRotationWeight = headIKRotationWeight;
            }

            //Left Arm
            if (leftArmLimb)
            {
                if (enableLeftArmLimb)
                {
                    leftArmLimb.solver.IKPositionWeight = leftArmLimbPositionWeight;
                    leftArmLimb.solver.IKRotationWeight = leftArmLimbRotationWeight;
                }
            }

            //Right Arm
            if (rightArmLimb)
            {
                if (enableRightArmLimb)
                {
                    rightArmLimb.solver.IKPositionWeight = rightArmLimbPositionWeight;
                    rightArmLimb.solver.IKRotationWeight = rightArmLimbRotationWeight;
                }
            }

            //Grounder
            if (grounderIK)
            {
                grounderIK.weight = grounderIKWeight;
            }

            //Left Leg
            if (leftLegLimb)
            {
                leftLegLimb.solver.IKRotationWeight = leftLegLimbRotationWeight;
            }

            //Right Leg
            if (rightLegLimb)
            {
                rightLegLimb.solver.IKRotationWeight = rightLegLimbRotationWeight;
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

        private void IKUpdate()
        {
            UpdateIKWeights();
        }

        private void SetInitialIKWeights()
        {
            InitialHipIKRotationWeight = hipIKRotationWeight;
            InitialHipIKPositionWeight = hipIKPositionWeight;
            InitialHeadIKPositionWeight = headIKPositionWeight;
            InitialHeadIKRotationWeight = headIKRotationWeight;
            InitialLeftArmLimbPositionWeight = leftArmLimbPositionWeight;
            InitialLeftArmLimbRotationWeight = leftArmLimbRotationWeight;
            InitialRightArmLimbPositionWeight = rightArmLimbPositionWeight;
            InitialRightArmLimbRotationWeight = rightArmLimbRotationWeight;
            InitialGrounderIKWeight = grounderIKWeight;
            InitialLeftLegLimbRotationWeight = leftLegLimbRotationWeight;
            InitialRightLegLimbRotationWeight = rightLegLimbRotationWeight;
            InitialSpineIKRotationWeight = spineIKRotationWeight;
            InitialSpineIKPositionWeight = spineIKPositionWeight;
            InitialNeckIKPositionWeight = neckIKPositionWeight;
            InitialNeckIKRotationWeight = neckIKRotationWeight;
        }

        private void IkWeightSensorSubscription()
        {
            hipIKRotationWeight = Mathf.Lerp(0, InitialHipIKRotationWeight, SensorAmount);
            hipIKPositionWeight = Mathf.Lerp(0, InitialHipIKPositionWeight, SensorAmount);
            headIKPositionWeight = Mathf.Lerp(0, InitialHeadIKPositionWeight, SensorAmount);
            headIKRotationWeight = Mathf.Lerp(0, InitialHeadIKRotationWeight, SensorAmount);
            leftArmLimbPositionWeight = Mathf.Lerp(0, InitialLeftArmLimbPositionWeight, SensorAmount);
            leftArmLimbRotationWeight = Mathf.Lerp(0, InitialLeftArmLimbRotationWeight, SensorAmount);
            rightArmLimbPositionWeight = Mathf.Lerp(0, InitialRightArmLimbRotationWeight, SensorAmount);
            rightArmLimbRotationWeight = Mathf.Lerp(0, InitialRightArmLimbRotationWeight, SensorAmount);
            grounderIKWeight = Mathf.Lerp(0, InitialGrounderIKWeight, SensorAmount);
            leftLegLimbRotationWeight = Mathf.Lerp(0, InitialLeftLegLimbRotationWeight, SensorAmount);
            rightLegLimbRotationWeight = Mathf.Lerp(0, InitialRightLegLimbRotationWeight, SensorAmount);
            spineIKPositionWeight = Mathf.Lerp(0, InitialSpineIKPositionWeight, SensorAmount);
            spineIKRotationWeight = Mathf.Lerp(0, InitialSpineIKRotationWeight, SensorAmount);
            neckIKRotationWeight = Mathf.Lerp(0, InitialNeckIKRotationWeight, SensorAmount);
            neckIKPositionWeight = Mathf.Lerp(0, InitialNeckIKPositionWeight, SensorAmount);
        }

        private void LegacyAwake()
        {
            DataMapper = FindObjectOfType<MrPuppetDataMapper>();
            HubConnection = FindObjectOfType<MrPuppetHubConnection>();

            HipSpawnPosition = HipTranslation.localPosition;
            HipSpawnRotation = HipRotation.rotation;
            HeadSpawnRotation = Head.rotation;

            // snapshot bind poses of weighted influence targets
            foreach (var influence in WeightedInfluences)
            {
                influence.SnapshotSpawn();
            }

            ApplySensors = true;

            DataMapper.OnSubscribeEvent += SubscribeEventIKButtPuppet;

            foreach (JawTransformMapper jaw in gameObject.GetComponentsInChildren<JawTransformMapper>())
            {
                JawTransformComponents.Add(jaw);
            }
            foreach (JawBlendShapeMapper jaw in gameObject.GetComponentsInChildren<JawBlendShapeMapper>())
            {
                JawBlendshapeComponents.Add(jaw);
            }

            if (gameObject.GetComponentInChildren<Animator>() != null)
            {
                _Animator = gameObject.GetComponentInChildren<Animator>();
                SetInitialIKWeights();
                IkWeightSensorSubscription();
            }


        }

        private void LegacyUpdate()
        {
            if (ApplySensors)
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

                if (_Animator)
                {
                    if (SensorAmount != 1f && SensorAmount != 0f)
                    {
                        IkWeightSensorSubscription();
                    }
                    _Animator.SetLayerWeight(1, Mathf.Abs(SensorAmount - 1f));
                }

                position = Vector3.Lerp(HipSpawnPosition, position, SensorAmount);
                UnsubscribeHipRotation = Quaternion.Slerp(HipSpawnRotation, (DataMapper.ElbowJoint.rotation * Quaternion.Inverse(DataMapper.AttachPose.ElbowRotation)) * HipSpawnRotation, SensorAmount);
                UnsubscribeHeadRotation = Quaternion.Slerp(HeadSpawnRotation, (DataMapper.WristJoint.rotation * Quaternion.Inverse(DataMapper.AttachPose.WristRotation)) * HeadSpawnRotation, SensorAmount);

                foreach (JawBlendShapeMapper Jaw in JawBlendshapeComponents) { Jaw.SensorAmount = SensorAmount; }
                foreach (JawTransformMapper Jaw in JawTransformComponents) { Jaw.SensorAmount = SensorAmount; }

                // clamp to XYZ extents (BEFORE smooth)
                position.Set(
                    LimitHipExtentX ? Mathf.Clamp(position.x, HipSpawnPosition.x - HipExtentX, HipSpawnPosition.x + HipExtentX) : position.x,
                                    LimitHipExtentY ? Mathf.Clamp(position.y, HipSpawnPosition.y - HipExtentY, HipSpawnPosition.y + HipExtentY) : position.y,
                                    LimitHipExtentZ ? Mathf.Clamp(position.z, HipSpawnPosition.z - HipExtentZ, HipSpawnPosition.z + HipExtentZ) : position.z
                                );

                // smoothly apply changes to position
                HipTranslation.localPosition = Vector3.SmoothDamp(HipTranslation.localPosition, position, ref PositionVelocity, PositionSpeed);

                // apply rotation deltas to bind pose
                HipRotation.rotation = Quaternion.Slerp(HipTranslation.rotation, UnsubscribeHipRotation, RotationSpeed * Time.deltaTime);
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
        #endregion
    }

    #region Classes

    #endregion

    #region Enums

    #endregion
}
