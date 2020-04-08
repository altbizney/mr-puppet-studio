using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using RootMotion.FinalIK;

namespace MrPuppet {
    public class IKButtPuppet : MonoBehaviour {

        #region Static Variables

        #endregion

        #region Public Variables
        #region Legacy
        [Title("Legacy")]
        public Transform HipRotation;
        public Transform HipTranslation;
        public Transform Head;
        // private Transform HipProxy;
        // private Transform HeadProxy;

        public List<WeightedInfluence> WeightedInfluences = new List<WeightedInfluence>();

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

        // performer attach position
        private bool AttachPoseSet = false;
        private Quaternion AttachPoseElbowRotation;
        private Quaternion AttachPoseWristRotation;
        private Vector3 AttachPoseElbowPosition;

        // spawn position of proxy geo
        private Vector3 HipSpawnPosition;
        private Quaternion HipSpawnRotation;
        private Quaternion HeadSpawnRotation;

        private Vector3 PositionVelocity;
        #endregion

        #region Unity Methods
        private void Awake() {
            LegacyAwake();
        }

        private void Update() {
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
        private void AssignIKNodes() {
            List<IKTag> tags = new List<IKTag>();
            foreach (Transform t in ikNodes) {
                tags.AddRange(t.GetComponentsInChildren<IKTag>());
            }
            foreach (IKTag tag in tags) {
                if (tag.iKTagId == IKTagId.Hip) {
                    hipIK = tag.GetComponent<TrigonometricIK>();
                    if (hipIK && hipIK.solver.bone3.transform) {
                        hipIK.transform.position = hipIK.solver.bone3.transform.position;
                        hipIK.transform.rotation = rigNode.rotation;
                    }
                }
                if (tag.iKTagId == IKTagId.Spine) {
                    spineIK = tag.GetComponent<TrigonometricIK>();
                    if (spineIK && spineIK.solver.bone3.transform) {
                        spineIK.transform.position = spineIK.solver.bone3.transform.position;
                        spineIK.transform.rotation = rigNode.rotation;
                    }
                }
                if (tag.iKTagId == IKTagId.Neck) {
                    neckIK = tag.GetComponent<TrigonometricIK>();
                    if (neckIK && neckIK.solver.bone3.transform) {
                        neckIK.transform.position = neckIK.solver.bone3.transform.position;
                        neckIK.transform.rotation = rigNode.rotation;
                    }
                }
                if (tag.iKTagId == IKTagId.Head) {
                    headIK = tag.GetComponent<TrigonometricIK>();
                    if (headIK && headIK.solver.bone3.transform) {
                        headIK.transform.position = headIK.solver.bone3.transform.position;
                        headIK.transform.rotation = rigNode.rotation;
                    }
                }
                if (tag.iKTagId == IKTagId.LeftArm && enableLeftArmLimb) {
                    leftArmLimb = tag.GetComponent<LimbIK>();
                    if (leftArmLimb && leftArmLimb.solver.bone1.transform) {
                        leftArmLimb.transform.position = leftArmLimb.solver.bone1.transform.position;
                        leftArmLimb.transform.rotation = rigNode.rotation;
                    }
                }
                if (tag.iKTagId == IKTagId.RightArm && enableRightArmLimb) {
                    rightArmLimb = tag.GetComponent<LimbIK>();
                    if (rightArmLimb && rightArmLimb.solver.bone1.transform) {
                        rightArmLimb.transform.position = rightArmLimb.solver.bone1.transform.position;
                        rightArmLimb.transform.rotation = rigNode.rotation;
                    }
                }
                if (tag.iKTagId == IKTagId.Grounder) {
                    grounderIK = tag.GetComponent<GrounderIK>();
                }
                if (tag.iKTagId == IKTagId.LeftLeg) {
                    leftLegLimb = tag.GetComponent<LimbIK>();
                    if (leftLegLimb && leftArmLimb.solver.bone3.transform) {
                        leftLegLimb.transform.position = leftLegLimb.solver.bone3.transform.position;
                        leftLegLimb.transform.rotation = rigNode.rotation;
                    }
                }
                if (tag.iKTagId == IKTagId.RightLeg) {
                    rightLegLimb = tag.GetComponent<LimbIK>();
                    if (rightLegLimb && rightArmLimb.solver.bone3.transform) {
                        rightLegLimb.transform.position = rightLegLimb.solver.bone3.transform.position;
                        rightLegLimb.transform.rotation = rigNode.rotation;
                    }
                }
            }
        }

        [Button("Assign IK Bones", 25, ButtonStyle.Box)]
        [GUIColor(1f, 1f, 0f)]
        [DisableInPlayMode()]
        private void AssignIKBones() {
            IKTag[] tags = rigNode.GetComponentsInChildren<IKTag>();

            //Hip
            if (hipIK) {
                foreach (IKTag tag in tags) {
                    if (tag.iKTagId == IKTagId.Hip) {
                        hipIK.solver.target = hipIKTarget;
                        switch (tag.chainId) {
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
            if (spineIK) {
                foreach (IKTag tag in tags) {
                    if (tag.iKTagId == IKTagId.Spine) {
                        spineIK.solver.target = spineIKTarget;
                        switch (tag.chainId) {
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
            if (neckIK) {
                foreach (IKTag tag in tags) {
                    if (tag.iKTagId == IKTagId.Neck) {
                        neckIK.solver.target = neckIKTarget;
                        switch (tag.chainId) {
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
            if (headIK) {
                foreach (IKTag tag in tags) {
                    if (tag.iKTagId == IKTagId.Head) {
                        headIK.solver.target = headIKTarget;
                        switch (tag.chainId) {
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
            if (enableLeftArmLimb) {
                foreach (IKTag tag in tags) {
                    if (tag.iKTagId == IKTagId.LeftArm) {
                        leftArmLimb.solver.target = leftArmTarget;
                        switch (tag.chainId) {
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
            if (enableRightArmLimb) {
                foreach (IKTag tag in tags) {
                    if (tag.iKTagId == IKTagId.RightArm) {
                        rightArmLimb.solver.target = rightArmTarget;
                        switch (tag.chainId) {
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
            if (grounderIK) {
                foreach (IKTag tag in tags) {
                    if (tag.iKTagId == IKTagId.Grounder) {
                        switch (tag.chainId) {
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
            if (leftLegLimb) {
                foreach (IKTag tag in tags) {
                    if (tag.iKTagId == IKTagId.LeftLeg) {
                        switch (tag.chainId) {
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
            if (rightLegLimb) {
                foreach (IKTag tag in tags) {
                    if (tag.iKTagId == IKTagId.RightLeg) {
                        switch (tag.chainId) {
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
        private void UpdateIKWeights() {
            //Hip
            if (hipIK) {
                hipIK.solver.IKPositionWeight = hipIKPositionWeight;
                hipIK.solver.IKRotationWeight = hipIKRotationWeight;
            }

            //Spine
            if (spineIK) {
                spineIK.solver.IKPositionWeight = spineIKPositionWeight;
                spineIK.solver.IKRotationWeight = spineIKRotationWeight;
            }

            //Neck
            if (neckIK) {
                neckIK.solver.IKPositionWeight = neckIKPositionWeight;
                neckIK.solver.IKRotationWeight = neckIKRotationWeight;
            }

            //Head
            if (headIK) {
                headIK.solver.IKPositionWeight = headIKPositionWeight;
                headIK.solver.IKRotationWeight = headIKRotationWeight;
            }

            //Left Arm
            if (leftArmLimb) {
                if (enableLeftArmLimb) {
                    leftArmLimb.solver.IKPositionWeight = leftArmLimbPositionWeight;
                    leftArmLimb.solver.IKRotationWeight = leftArmLimbRotationWeight;
                }
            }

            //Right Arm
            if (rightArmLimb) {
                if (enableRightArmLimb) {
                    rightArmLimb.solver.IKPositionWeight = rightArmLimbPositionWeight;
                    rightArmLimb.solver.IKRotationWeight = rightArmLimbRotationWeight;
                }
            }

            //Grounder
            if (grounderIK) {
                grounderIK.weight = grounderIKWeight;
            }

            //Left Leg
            if (leftLegLimb) {
                leftLegLimb.solver.IKRotationWeight = leftLegLimbRotationWeight;
            }

            //Right Leg
            if (rightLegLimb) {
                rightLegLimb.solver.IKRotationWeight = rightLegLimbRotationWeight;
            }
        }

        private void IKUpdate() {
            UpdateIKWeights();
        }

        [Button(ButtonSizes.Large)]
        [GUIColor(0f, 1f, 0f)]
        [DisableInEditorMode()]
        private void GrabAttachPose() {
            AttachPoseSet = true;

            // grab the attach position of the elbow joint
            AttachPoseElbowPosition = DataMapper.ElbowJoint.position;

            // grab the attach rotation of the joints
            AttachPoseElbowRotation = DataMapper.ElbowJoint.rotation;
            AttachPoseWristRotation = DataMapper.WristJoint.rotation;

            // send attach poses to weighted infleunces
            foreach (var influence in WeightedInfluences) {
                influence.SnapshotAttach(AttachPoseElbowRotation, AttachPoseWristRotation);
            }

            HubConnection.SendSocketMessage("COMMAND;ATTACH;" + AttachPoseToString());
        }

        private string AttachPoseToString() {
            string packet = "";

            packet += AttachPoseElbowPosition.x + "," + AttachPoseElbowPosition.y + "," + AttachPoseElbowPosition.z + ";";
            packet += AttachPoseElbowRotation.x + "," + AttachPoseElbowRotation.y + "," + AttachPoseElbowRotation.z + "," + AttachPoseElbowRotation.w + ";";
            packet += AttachPoseWristRotation.x + "," + AttachPoseWristRotation.y + "," + AttachPoseWristRotation.z + "," + AttachPoseWristRotation.w;

            return packet;
        }

        private void AttachPoseFromString(string[] elbowPos, string[] elbowRot, string[] wristRot) {
            AttachPoseElbowPosition = new Vector3(float.Parse(elbowPos[0]), float.Parse(elbowPos[1]), float.Parse(elbowPos[2]));
            AttachPoseElbowRotation = new Quaternion(float.Parse(elbowRot[0]), float.Parse(elbowRot[1]), float.Parse(elbowRot[2]), float.Parse(elbowRot[3]));
            AttachPoseWristRotation = new Quaternion(float.Parse(wristRot[0]), float.Parse(wristRot[1]), float.Parse(wristRot[2]), float.Parse(wristRot[3]));

            AttachPoseSet = true;
        }

        private void LegacyAwake() {
            DataMapper = FindObjectOfType<MrPuppetDataMapper>();
            HubConnection = FindObjectOfType<MrPuppetHubConnection>();

            // // clone proxy geo
            // HipProxy = new GameObject("Proxy:" + Hip.name).transform;
            // HipProxy.SetPositionAndRotation(Hip.position, Hip.rotation);
            // HipProxy.SetParent(Hip.parent, false);

            // HeadProxy = new GameObject("Proxy:" + Head.name).transform;
            // HeadProxy.SetPositionAndRotation(Head.position, Head.rotation);
            // HeadProxy.SetParent(Head.parent, false);

            HipSpawnPosition = HipTranslation.localPosition;
            HipSpawnRotation = HipRotation.rotation;
            HeadSpawnRotation = Head.rotation;

            // snapshot bind poses of weighted influence targets
            foreach (var influence in WeightedInfluences) {
                influence.SnapshotSpawn();
            }
        }

        private void LegacyUpdate() {
            if (AttachPoseSet) {
                // apply position delta to bind pose
                Vector3 position = HipSpawnPosition + (DataMapper.ElbowJoint.position - AttachPoseElbowPosition);

                // clamp to XYZ extents (BEFORE smooth)
                position.Set(
                    LimitHipExtentX ? Mathf.Clamp(position.x, HipSpawnPosition.x - HipExtentX, HipSpawnPosition.x + HipExtentX) : position.x,
                    LimitHipExtentY ? Mathf.Clamp(position.y, HipSpawnPosition.y - HipExtentY, HipSpawnPosition.y + HipExtentY) : position.y,
                    LimitHipExtentZ ? Mathf.Clamp(position.z, HipSpawnPosition.z - HipExtentZ, HipSpawnPosition.z + HipExtentZ) : position.z
                );

                // smoothly apply changes to position
                HipTranslation.localPosition = Vector3.SmoothDamp(HipTranslation.localPosition, position, ref PositionVelocity, PositionSpeed);

                // apply rotation deltas to bind pose
                HipRotation.rotation = Quaternion.Slerp(HipRotation.rotation, (DataMapper.ElbowJoint.rotation * Quaternion.Inverse(AttachPoseElbowRotation)) * HipSpawnRotation, RotationSpeed * Time.deltaTime);
                Head.rotation = Quaternion.Slerp(Head.rotation, (DataMapper.WristJoint.rotation * Quaternion.Inverse(AttachPoseWristRotation)) * HeadSpawnRotation, RotationSpeed * Time.deltaTime);

                if (EnableJawHeadMixer) {
                    switch (JawHeadRotate) {
                        case JawHeadAxis.x:
                            Head.Rotate(Mathf.Lerp(0f, JawHeadMaxExtent, DataMapper.JawPercent), 0f, 0f, Space.Self);
                            break;

                        case JawHeadAxis.y:
                            Head.Rotate(0f, Mathf.Lerp(0f, JawHeadMaxExtent, DataMapper.JawPercent), 0f, Space.Self);
                            break;

                        case JawHeadAxis.z:
                            Head.Rotate(0f, 0f, Mathf.Lerp(0f, JawHeadMaxExtent, DataMapper.JawPercent), Space.Self);
                            break;
                    }
                }

                // apply weighted influences
                foreach (var influence in WeightedInfluences) {
                    influence.Update(DataMapper, RotationSpeed);
                }
            }

            if (Input.GetKeyDown(KeyCode.A)) {
                GrabAttachPose();
            }
        }
        #endregion
    }

    #region Classes
    [Serializable]
    public class WeightedInfluence {
        public MrPuppetDataMapper.Joint joint;
        public Transform target;
        // private Transform proxy;

        [Range(0f, 1f)]
        public float amount = 1f;

        private Quaternion attach;
        private Quaternion spawn;
        private Quaternion full;
        private Quaternion weighted;

        public void SnapshotSpawn() {
            // proxy = new GameObject("Proxy:" + target.name).transform;
            // proxy.SetPositionAndRotation(target.position, target.rotation);
            // proxy.SetParent(target.parent, false);

            spawn = target.rotation;
        }

        public void SnapshotAttach(Quaternion elbow, Quaternion wrist) {
            switch (joint) {
                case MrPuppetDataMapper.Joint.Elbow:
                    attach = elbow;
                    return;
                case MrPuppetDataMapper.Joint.Wrist:
                    attach = wrist;
                    return;
            }
        }

        public void Update(MrPuppetDataMapper DataMapper, float RotationSpeed) {
            if (!target)
                return;

            // calculate fully blended extent
            full = (DataMapper.GetJoint(joint).rotation * Quaternion.Inverse(attach)) * spawn;

            // calculate weighted rotation
            weighted = Quaternion.Slerp(spawn, full, amount);

            // apply with smoothing
            target.rotation = Quaternion.Slerp(target.rotation, weighted, RotationSpeed * Time.deltaTime);
        }

        public void OnDrawGizmos() {
            if (!target)
                return;

            Debug.DrawRay(target.position, target.up * 0.5f, Color.green, 0f, false);
            Debug.DrawRay(target.position, target.right * 0.5f, Color.red, 0f, false);
            Debug.DrawRay(target.position, target.forward * 0.5f, Color.blue, 0f, false);
        }
    }
    #endregion

    #region Enums

    #endregion
}
