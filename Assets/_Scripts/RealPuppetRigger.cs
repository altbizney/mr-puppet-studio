using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Thinko
{

    public class RealPuppetRigger : MonoBehaviour
    {

        [Header("Joint Setup")]

        public List<Transform> jointChainRoots = new List<Transform>();

        public JointType jointType = JointType.configurableJoint;

        [Header("Use jointChainRoots[0] settings for all joints")]
        public bool overrideJointSettings;

        [Header("Configurable Joint Settings")]

        [Header("Character Joint Settings")]

        [Header("Final IK Joint Settings")]


        RealPuppet puppet;


        void Start()
        {

            puppet = GetComponent<RealPuppet>();
            SetupJoints();
        }

        void SetupJoints() {
        
            foreach (Transform jointChainRoot in jointChainRoots)
            {
                SetupJointChain(jointChainRoot, jointType);
            }

        } // SetupJoints()

        void SetupJointChain(Transform jointChainRoot, JointType jt) {

            if (jt == JointType.configurableJoint) {
                SetupConfigurableJointChain(jointChainRoot);
                SetupRigidbodies(jointChainRoot);
            }

            if (jt == JointType.characterJoint)
            {
                SetupCharacterJointChain(jointChainRoot);
                SetupRigidbodies(jointChainRoot);

            }

            if (jt == JointType.FinalIK)
            {
                SetupFinalIKJointChain(jointChainRoot);
            }
            if (jt == JointType.Hybrid)
            {
                SetupHybridJointChain(jointChainRoot);
            }

        }
        void SetupCharacterJointChain(Transform jointChainRoot) {

            CharacterJoint masterJoint = jointChainRoot.GetComponent<CharacterJoint>();

            if (masterJoint == null)
            {
                masterJoint = jointChainRoot.GetChild(0).GetComponent<CharacterJoint>();
            }

            // Apply the hip bone settings to tail settings as well
            if (overrideJointSettings)
            {
                masterJoint = jointChainRoots[0].GetComponent<CharacterJoint>();

                if (masterJoint == null) {
                    masterJoint = jointChainRoots[0].GetChild(0).GetComponent<CharacterJoint>();
                }

            }
            //Debug.Log("Master Joint is " + masterJoint.name, masterJoint);

            CharacterJoint[] childJoints = jointChainRoot.GetComponentsInChildren<CharacterJoint>();


            foreach (CharacterJoint joint in childJoints)
            {
                SetupCharacterJoint(masterJoint, joint);

            }

        }
        void SetupConfigurableJointChain(Transform jointChainRoot)
        {

            ConfigurableJoint masterJoint = jointChainRoot.GetComponent<ConfigurableJoint>();

            if (masterJoint == null)
            {
                masterJoint = jointChainRoot.GetChild(0).GetComponent<ConfigurableJoint>();
            }

            if (overrideJointSettings)
            {
                masterJoint = jointChainRoots[0].GetComponent<ConfigurableJoint>();

                if (masterJoint == null)
                {
                    masterJoint = jointChainRoots[0].GetChild(0).GetComponent<ConfigurableJoint>();
                }


            }
            Debug.Log("Master Joint is " + masterJoint.name, masterJoint);

            ConfigurableJoint[] childJoints = jointChainRoot.GetComponentsInChildren<ConfigurableJoint>();


            foreach (ConfigurableJoint joint in childJoints)
            {
                SetupConfigurableJoint(masterJoint, joint);

            }

        }
        void SetupFinalIKJointChain(Transform jointChainRoot)
        {

            Debug.Log("Final IK Not ready yet!");

        }
        void SetupHybridJointChain(Transform jointChainRoot)
        {

            Debug.Log("Hybrid not ready yet!");

        }
        void SetupRigidbodies(Transform jointChainRoot)
        {

            Rigidbody masterRigidbody = jointChainRoot.GetComponent<Rigidbody>();

            if (masterRigidbody == null)
            {
                masterRigidbody = jointChainRoot.GetChild(0).GetComponent<Rigidbody>();
            }


            Rigidbody[] childRigidbodies = jointChainRoot.GetComponentsInChildren<Rigidbody>();


            foreach (Rigidbody rb in childRigidbodies)
            {
                SetupRigidbody(masterRigidbody, rb);

            }

        }

        void SetupConfigurableJoint(ConfigurableJoint masterJoint, ConfigurableJoint jointToSetup) {

            jointToSetup.anchor = masterJoint.anchor;
            jointToSetup.axis = masterJoint.axis;
            jointToSetup.secondaryAxis = masterJoint.secondaryAxis;

            jointToSetup.xMotion = masterJoint.xMotion;
            jointToSetup.yMotion = masterJoint.yMotion;
            jointToSetup.zMotion = masterJoint.zMotion;

            jointToSetup.angularXMotion = masterJoint.angularXMotion;
            jointToSetup.angularYMotion = masterJoint.angularYMotion;
            jointToSetup.angularZMotion = masterJoint.angularZMotion;

            jointToSetup.linearLimitSpring = masterJoint.linearLimitSpring;
            jointToSetup.linearLimit = masterJoint.linearLimit;

            jointToSetup.angularXLimitSpring = masterJoint.angularXLimitSpring;
            jointToSetup.lowAngularXLimit = masterJoint.lowAngularXLimit;
            jointToSetup.highAngularXLimit = masterJoint.highAngularXLimit;
            jointToSetup.angularYZLimitSpring = masterJoint.angularYZLimitSpring;
            jointToSetup.angularYLimit = masterJoint.angularYLimit;
            jointToSetup.angularZLimit = masterJoint.angularZLimit;

        }
        void SetupCharacterJoint(CharacterJoint masterJoint, CharacterJoint jointToSetup)
        {

            jointToSetup.anchor = masterJoint.anchor;
            jointToSetup.axis = masterJoint.axis;
            jointToSetup.swingAxis = masterJoint.swingAxis;

            // Twist Limit Spring
            SoftJointLimitSpring jointTwistLimitSpring = new SoftJointLimitSpring();
            jointTwistLimitSpring.spring = masterJoint.twistLimitSpring.spring;
            jointTwistLimitSpring.damper = masterJoint.twistLimitSpring.damper;

            jointToSetup.twistLimitSpring = jointTwistLimitSpring;

            // Low Twist Limit
            SoftJointLimit jointLowTwistLimit = new SoftJointLimit();
            jointLowTwistLimit.limit = masterJoint.lowTwistLimit.limit;
            jointLowTwistLimit.bounciness = masterJoint.lowTwistLimit.bounciness;
            jointLowTwistLimit.contactDistance = masterJoint.lowTwistLimit.contactDistance;

            jointToSetup.lowTwistLimit = jointLowTwistLimit;

            // High Twist Limit
            SoftJointLimit jointHighTwistLimit = new SoftJointLimit();
            jointHighTwistLimit.limit = masterJoint.highTwistLimit.limit;
            jointHighTwistLimit.bounciness = masterJoint.highTwistLimit.bounciness;
            jointHighTwistLimit.contactDistance = masterJoint.highTwistLimit.contactDistance;

            jointToSetup.highTwistLimit = jointHighTwistLimit;


            // Swing Limit Spring
            SoftJointLimitSpring jointSwingLimitSpring = new SoftJointLimitSpring();
            jointSwingLimitSpring.spring = masterJoint.swingLimitSpring.spring;
            jointSwingLimitSpring.damper = masterJoint.twistLimitSpring.damper;

            jointToSetup.swingLimitSpring = jointSwingLimitSpring;

            // Swing 1 Limit
            SoftJointLimit jointSwing1Limit = new SoftJointLimit();
            jointSwing1Limit.limit = masterJoint.swing1Limit.limit;
            jointSwing1Limit.bounciness = masterJoint.swing1Limit.bounciness;
            jointSwing1Limit.contactDistance = masterJoint.swing1Limit.contactDistance;

            jointToSetup.swing1Limit = jointSwing1Limit;

            // Swing 2 Limit
            SoftJointLimit jointSwing2Limit = new SoftJointLimit();
            jointSwing2Limit.limit = masterJoint.swing2Limit.limit;
            jointSwing2Limit.bounciness = masterJoint.swing2Limit.bounciness;
            jointSwing2Limit.contactDistance = masterJoint.swing2Limit.contactDistance;

            jointToSetup.swing2Limit = jointSwing2Limit;


            jointToSetup.enableProjection = masterJoint.enableProjection;
            jointToSetup.enableCollision = masterJoint.enableCollision;
            jointToSetup.enablePreprocessing = masterJoint.enablePreprocessing;
            jointToSetup.massScale = masterJoint.massScale;
            jointToSetup.connectedMassScale = masterJoint.connectedMassScale;

        }
        void SetupFinalIKJoint()
        {


        }

        void SetupRigidbody(Rigidbody masterRigidbody, Rigidbody rb) {

            rb.mass = masterRigidbody.mass;
            rb.drag = masterRigidbody.drag;
            rb.angularDrag = masterRigidbody.angularDrag;
            rb.useGravity = masterRigidbody.useGravity;
            rb.isKinematic = masterRigidbody.isKinematic;

        }


        public void StartRiggingPuppet()
        {
            StartCoroutine(RigPuppet());

        }

        private IEnumerator RigPuppet()
        {

            SetupJoints();
            yield return new WaitForEndOfFrame();


        }

    }
}

public enum JointType { characterJoint, configurableJoint, FinalIK, Hybrid }