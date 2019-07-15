using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet
{
    public class ButtPuppet : MonoBehaviour
    {
        [Serializable]
        public class WeightedInfluence
        {
            public MrPuppetDataMapper.Joint joint;
            public Transform target;

            [Range(0f, 1f)]
            public float amount = 1f;

            private Quaternion attach;
            private Quaternion bind;
            private Quaternion full;
            private Quaternion weighted;

            public void SnapshotBind()
            {
                bind = target.rotation;
            }

            public void SnapshotAttach(Quaternion elbow, Quaternion wrist)
            {
                switch (joint)
                {
                    case MrPuppetDataMapper.Joint.Elbow:
                        attach = elbow;
                        return;
                    case MrPuppetDataMapper.Joint.Wrist:
                        attach = wrist;
                        return;
                }
            }

            public void Update(MrPuppetDataMapper DataMapper, float RotationSpeed)
            {
                if (!target) return;

                // calculate fully blended extent
                full = (DataMapper.GetJoint(joint).rotation * Quaternion.Inverse(attach)) * bind;

                // calculate weighted rotation
                weighted = Quaternion.Slerp(bind, full, amount);

                // apply with smoothing
                target.rotation = Quaternion.Slerp(target.rotation, weighted, RotationSpeed * Time.smoothDeltaTime);
            }
        }

        [Required]
        public MrPuppetDataMapper DataMapper;

        private bool AttachPoseSet = false;
        private Quaternion AttachPoseElbowRotation;
        private Quaternion AttachPoseWristRotation;
        private Vector3 AttachPoseElbowPosition;

        private Vector3 BindPoseButtPosition;
        private Quaternion BindPoseButtRotation;
        private Quaternion BindPoseNeckRotation;

        public Transform Butt;
        public Transform Neck;

        public List<WeightedInfluence> WeightedInfluences = new List<WeightedInfluence>();

        [MinValue(0f)]
        public float RotationSpeed = 7f;
        [MinValue(0f)]
        public float PositionSpeed = 0.1f;
        private Vector3 PositionVelocity;

        private void OnValidate()
        {
            if (!DataMapper) DataMapper = FindObjectOfType<MrPuppetDataMapper>();
        }

        private bool ApplicationIsPlaying()
        {
            return Application.isPlaying;
        }

        [Button(ButtonSizes.Large)]
        [GUIColor(0f, 1f, 0f)]
        [EnableIf(nameof(ApplicationIsPlaying))]
        public void GrabAttachPose()
        {
            AttachPoseSet = true;

            // grab the attach position of the elbow joint
            AttachPoseElbowPosition = DataMapper.ElbowJoint.position;

            // grab the attach rotation of the joints
            AttachPoseElbowRotation = DataMapper.ElbowJoint.rotation;
            AttachPoseWristRotation = DataMapper.WristJoint.rotation;

            // send attach poses to weighted infleunces
            foreach (var influence in WeightedInfluences)
            {
                influence.SnapshotAttach(AttachPoseElbowRotation, AttachPoseWristRotation);
            }
        }

        private void Awake()
        {
            BindPoseButtPosition = Butt.position;
            BindPoseButtRotation = Butt.rotation;
            BindPoseNeckRotation = Neck.rotation;

            // snapshot bind poses of weighted influence targets
            foreach (var influence in WeightedInfluences)
            {
                influence.SnapshotBind();
            }
        }

        private void Update()
        {
            if (AttachPoseSet)
            {
                // apply position delta to bind pose
                Butt.position = Vector3.SmoothDamp(Butt.position, BindPoseButtPosition + (DataMapper.ElbowJoint.position - AttachPoseElbowPosition), ref PositionVelocity, PositionSpeed);

                // apply rotation deltas to bind pose
                Butt.rotation = Quaternion.Slerp(Butt.rotation, (DataMapper.ElbowJoint.rotation * Quaternion.Inverse(AttachPoseElbowRotation)) * BindPoseButtRotation, RotationSpeed * Time.smoothDeltaTime);
                Neck.rotation = Quaternion.Slerp(Neck.rotation, (DataMapper.WristJoint.rotation * Quaternion.Inverse(AttachPoseWristRotation)) * BindPoseNeckRotation, RotationSpeed * Time.smoothDeltaTime);

                // apply weighted influences
                foreach (var influence in WeightedInfluences)
                {
                    influence.Update(DataMapper, RotationSpeed);
                }
            }
        }

        private void OnDrawGizmos()
        {
            Debug.DrawRay(Butt.position, Butt.up * 0.5f, Color.green, 0f, false);
            Debug.DrawRay(Neck.position, Neck.up * 0.5f, Color.green, 0f, false);
            foreach (var influence in WeightedInfluences)
                Debug.DrawRay(influence.target.position, influence.target.up * 0.5f, Color.green, 0f, false);

            Debug.DrawRay(Butt.position, Butt.right * 0.5f, Color.red, 0f, false);
            Debug.DrawRay(Neck.position, Neck.right * 0.5f, Color.red, 0f, false);
            foreach (var influence in WeightedInfluences)
                Debug.DrawRay(influence.target.position, influence.target.right * 0.5f, Color.red, 0f, false);

            Debug.DrawRay(Butt.position, Butt.forward * 0.5f, Color.blue, 0f, false);
            Debug.DrawRay(Neck.position, Neck.forward * 0.5f, Color.blue, 0f, false);
            foreach (var influence in WeightedInfluences)
                Debug.DrawRay(influence.target.position, influence.target.forward * 0.5f, Color.blue, 0f, false);
        }
    }
}
