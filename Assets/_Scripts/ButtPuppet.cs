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
        private Transform ButtProxy;
        public Transform Neck;
        private Transform NeckProxy;

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
            // foreach (var influence in WeightedInfluences)
            // {
            //     influence.SnapshotAttach(AttachPoseElbowRotation, AttachPoseWristRotation);
            // }
        }

        private void Awake()
        {
            // clone proxy geo
            ButtProxy = new GameObject("ButtProxy").transform;
            ButtProxy.SetPositionAndRotation(Butt.position, Butt.rotation);
            ButtProxy.SetParent(Butt.parent, false);

            NeckProxy = new GameObject("NeckProxy").transform;
            NeckProxy.SetPositionAndRotation(Neck.position, Neck.rotation);
            NeckProxy.SetParent(Neck.parent, false);

            BindPoseButtPosition = Butt.localPosition;
            BindPoseButtRotation = Butt.rotation;
            BindPoseNeckRotation = Neck.rotation;

            // snapshot bind poses of weighted influence targets
            // foreach (var influence in WeightedInfluences)
            // {
            //     influence.SnapshotBind();
            // }
        }

        private void Update()
        {
            if (AttachPoseSet)
            {
                // apply position delta to bind pose
                ButtProxy.localPosition = Vector3.SmoothDamp(ButtProxy.localPosition, BindPoseButtPosition + (DataMapper.ElbowJoint.position - AttachPoseElbowPosition), ref PositionVelocity, PositionSpeed);

                // apply rotation deltas to bind pose
                ButtProxy.rotation = Quaternion.Slerp(ButtProxy.rotation, (DataMapper.ElbowJoint.rotation * Quaternion.Inverse(AttachPoseElbowRotation)) * BindPoseButtRotation, RotationSpeed * Time.smoothDeltaTime);
                NeckProxy.rotation = Quaternion.Slerp(NeckProxy.rotation, (DataMapper.WristJoint.rotation * Quaternion.Inverse(AttachPoseWristRotation)) * BindPoseNeckRotation, RotationSpeed * Time.smoothDeltaTime);

                // apply weighted influences
                // foreach (var influence in WeightedInfluences)
                // {
                //     influence.Update(DataMapper, RotationSpeed);
                // }
            }
        }

        private void LateUpdate()
        {
            if (!(ButtProxy && NeckProxy)) return;

            Butt.position = Butt.position - (BindPoseButtPosition - ButtProxy.position);

            // Butt.rotation = ButtProxy.rotation;
            // Neck.rotation = NeckProxy.rotation;
        }

        private void OnDrawGizmos()
        {
            if (ButtProxy) Debug.DrawRay(ButtProxy.position, ButtProxy.up * 0.5f, Color.green, 0f, false);
            if (NeckProxy) Debug.DrawRay(NeckProxy.position, NeckProxy.up * 0.5f, Color.green, 0f, false);
            // foreach (var influence in WeightedInfluences)
            //     if (influence.target) Debug.DrawRay(influence.target.position, influence.target.up * 0.5f, Color.green, 0f, false);

            if (ButtProxy) Debug.DrawRay(ButtProxy.position, ButtProxy.right * 0.5f, Color.red, 0f, false);
            if (NeckProxy) Debug.DrawRay(NeckProxy.position, NeckProxy.right * 0.5f, Color.red, 0f, false);
            // foreach (var influence in WeightedInfluences)
            //     if (influence.target) Debug.DrawRay(influence.target.position, influence.target.right * 0.5f, Color.red, 0f, false);

            if (ButtProxy) Debug.DrawRay(ButtProxy.position, ButtProxy.forward * 0.5f, Color.blue, 0f, false);
            if (NeckProxy) Debug.DrawRay(NeckProxy.position, NeckProxy.forward * 0.5f, Color.blue, 0f, false);
            // foreach (var influence in WeightedInfluences)
            //     if (influence.target) Debug.DrawRay(influence.target.position, influence.target.forward * 0.5f, Color.blue, 0f, false);
        }
    }
}
