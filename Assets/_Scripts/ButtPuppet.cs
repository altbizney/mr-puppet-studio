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
            // private Transform proxy;

            [Range(0f, 1f)]
            public float amount = 1f;

            private Quaternion attach;
            private Quaternion spawn;
            private Quaternion full;
            private Quaternion weighted;

            public void SnapshotSpawn()
            {
                // proxy = new GameObject("Proxy:" + target.name).transform;
                // proxy.SetPositionAndRotation(target.position, target.rotation);
                // proxy.SetParent(target.parent, false);

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
                target.rotation = Quaternion.Slerp(target.rotation, weighted, RotationSpeed * Time.smoothDeltaTime);
            }

            public void OnDrawGizmos()
            {
                if (!target) return;

                Debug.DrawRay(target.position, target.up * 0.5f, Color.green, 0f, false);
                Debug.DrawRay(target.position, target.right * 0.5f, Color.red, 0f, false);
                Debug.DrawRay(target.position, target.forward * 0.5f, Color.blue, 0f, false);
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
        private Vector3 HipSpawnPosition;
        private Quaternion HipSpawnRotation;
        private Quaternion HeadSpawnRotation;

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
            // // clone proxy geo
            // HipProxy = new GameObject("Proxy:" + Hip.name).transform;
            // HipProxy.SetPositionAndRotation(Hip.position, Hip.rotation);
            // HipProxy.SetParent(Hip.parent, false);

            // HeadProxy = new GameObject("Proxy:" + Head.name).transform;
            // HeadProxy.SetPositionAndRotation(Head.position, Head.rotation);
            // HeadProxy.SetParent(Head.parent, false);

            HipSpawnPosition = Hip.position;
            HipSpawnRotation = Hip.rotation;
            HeadSpawnRotation = Head.rotation;

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
                // apply position delta to bind pose
                Vector3 position = HipSpawnPosition + (DataMapper.ElbowJoint.position - AttachPoseElbowPosition);

                // clamp to XYZ extents (BEFORE) smooth
                position.Set(
                    LimitHipExtentX ? Mathf.Clamp(position.x, HipSpawnPosition.x - HipExtentX, HipSpawnPosition.x + HipExtentX) : position.x,
                    LimitHipExtentY ? Mathf.Clamp(position.y, HipSpawnPosition.y - HipExtentY, HipSpawnPosition.y + HipExtentY) : position.y,
                    LimitHipExtentZ ? Mathf.Clamp(position.z, HipSpawnPosition.z - HipExtentZ, HipSpawnPosition.z + HipExtentZ) : position.z
                );

                // smoothly apply changes to position
                Hip.position = Vector3.SmoothDamp(Hip.position, position, ref PositionVelocity, PositionSpeed);

                // apply rotation deltas to bind pose
                Hip.rotation = Quaternion.Slerp(Hip.rotation, (DataMapper.ElbowJoint.rotation * Quaternion.Inverse(AttachPoseElbowRotation)) * HipSpawnRotation, RotationSpeed * Time.smoothDeltaTime);
                Head.rotation = Quaternion.Slerp(Head.rotation, (DataMapper.WristJoint.rotation * Quaternion.Inverse(AttachPoseWristRotation)) * HeadSpawnRotation, RotationSpeed * Time.smoothDeltaTime);

                // apply weighted influences
                foreach (var influence in WeightedInfluences)
                {
                    influence.Update(DataMapper, RotationSpeed);
                }
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
        }
    }
}
