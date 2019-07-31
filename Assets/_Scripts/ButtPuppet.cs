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
            private Transform proxy;

            [Range(0f, 1f)]
            public float amount = 1f;

            private Quaternion attach;
            private Quaternion spawn;
            private Quaternion full;
            private Quaternion weighted;

            public void SnapshotSpawn()
            {
                proxy = new GameObject("Proxy:" + target.name).transform;
                proxy.SetPositionAndRotation(target.position, target.rotation);
                proxy.SetParent(target.parent, false);

                spawn = target.rotation;
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
                full = (DataMapper.GetJoint(joint).rotation * Quaternion.Inverse(attach)) * spawn;

                // calculate weighted rotation
                weighted = Quaternion.Slerp(spawn, full, amount);

                // apply with smoothing
                proxy.rotation = Quaternion.Slerp(proxy.rotation, weighted, RotationSpeed * Time.smoothDeltaTime);
            }

            public void OnDrawGizmos()
            {
                if (!proxy) return;

                Debug.DrawRay(proxy.position, proxy.up * 0.5f, Color.green, 0f, false);
                Debug.DrawRay(proxy.position, proxy.right * 0.5f, Color.red, 0f, false);
                Debug.DrawRay(proxy.position, proxy.forward * 0.5f, Color.blue, 0f, false);
            }
        }

        [Required]
        public MrPuppetDataMapper DataMapper;

        // performer attach position
        private bool AttachPoseSet = false;
        private Quaternion AttachPoseElbowRotation;
        private Quaternion AttachPoseWristRotation;
        private Vector3 AttachPoseElbowPosition;

        // spawn position of proxy geo
        private Vector3 HipProxySpawnPosition;
        private Quaternion HipProxySpawnRotation;
        private Quaternion HeadProxySpawnRotation;

        public Transform Hip;
        public Transform Head;
        private Transform HipProxy;
        private Transform HeadProxy;

        public List<WeightedInfluence> WeightedInfluences = new List<WeightedInfluence>();

        [MinValue(0f)]
        public float RotationSpeed = 7f;
        [MinValue(0f)]
        public float PositionSpeed = 0.1f;
        private Vector3 PositionVelocity;

        public bool EnableHipPosition = true;

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
            // clone proxy geo
            HipProxy = new GameObject("Proxy:" + Hip.name).transform;
            HipProxy.SetPositionAndRotation(Hip.position, Hip.rotation);
            HipProxy.SetParent(Hip.parent, false);

            HeadProxy = new GameObject("Proxy:" + Head.name).transform;
            HeadProxy.SetPositionAndRotation(Head.position, Head.rotation);
            HeadProxy.SetParent(Head.parent, false);

            HipProxySpawnPosition = Hip.position;
            HipProxySpawnRotation = Hip.rotation;
            HeadProxySpawnRotation = Head.rotation;

            // snapshot bind poses of weighted influence targets
            foreach (var influence in WeightedInfluences)
            {
                influence.SnapshotSpawn();
            }
        }

        private void Update()
        {
            if (AttachPoseSet)
            {
                if (EnableHipPosition)
                {
                    // apply position delta to bind pose
                    HipProxy.position = Vector3.SmoothDamp(HipProxy.position, HipProxySpawnPosition + (DataMapper.ElbowJoint.position - AttachPoseElbowPosition), ref PositionVelocity, PositionSpeed);
                }

                // apply rotation deltas to bind pose
                HipProxy.rotation = Quaternion.Slerp(HipProxy.rotation, (DataMapper.ElbowJoint.rotation * Quaternion.Inverse(AttachPoseElbowRotation)) * HipProxySpawnRotation, RotationSpeed * Time.smoothDeltaTime);
                HeadProxy.rotation = Quaternion.Slerp(HeadProxy.rotation, (DataMapper.WristJoint.rotation * Quaternion.Inverse(AttachPoseWristRotation)) * HeadProxySpawnRotation, RotationSpeed * Time.smoothDeltaTime);

                // apply weighted influences
                foreach (var influence in WeightedInfluences)
                {
                    influence.Update(DataMapper, RotationSpeed);
                }
            }
        }

        // REMINDER: Change = Quaternion.Inverse(Last) * Current;

        private void LateUpdate()
        {
            if (!(HipProxy && HeadProxy)) return;

            if (EnableHipPosition) Hip.position = Hip.position - (HipProxySpawnPosition - HipProxy.position);

            Hip.rotation = Hip.rotation * (Quaternion.Inverse(Hip.rotation) * HipProxy.rotation);
            Head.rotation = Head.rotation * (Quaternion.Inverse(Head.rotation) * HeadProxy.rotation);

            // WIP: this basically is world-relative. also sort of busted on head....
            // Hip.rotation = Hip.rotation * (HipProxy.rotation * Quaternion.Inverse(HipProxySpawnRotation));
            // Head.rotation = Head.rotation * (HeadProxy.localRotation * Quaternion.Inverse(HeadProxySpawnRotation));
        }

        private void OnDrawGizmos()
        {
            if (HipProxy) Debug.DrawRay(HipProxy.position, HipProxy.up * 0.5f, Color.green, 0f, false);
            if (HeadProxy) Debug.DrawRay(HeadProxy.position, HeadProxy.up * 0.5f, Color.green, 0f, false);

            if (HipProxy) Debug.DrawRay(HipProxy.position, HipProxy.right * 0.5f, Color.red, 0f, false);
            if (HeadProxy) Debug.DrawRay(HeadProxy.position, HeadProxy.right * 0.5f, Color.red, 0f, false);

            if (HipProxy) Debug.DrawRay(HipProxy.position, HipProxy.forward * 0.5f, Color.blue, 0f, false);
            if (HeadProxy) Debug.DrawRay(HeadProxy.position, HeadProxy.forward * 0.5f, Color.blue, 0f, false);

            foreach (var influence in WeightedInfluences) influence.OnDrawGizmos();
        }
    }
}
