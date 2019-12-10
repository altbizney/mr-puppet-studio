using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPuppet.WIP
{
    public class ButtPuppet2 : MonoBehaviour
    {
        [ChildGameObjectsOnly]
        public Transform Hip;

        [ChildGameObjectsOnly]
        public Transform Head;

        private MrPuppetDataMapper DataMapper;

        private Quaternion SpawnHipRotation, SpawnHeadRotation;

        private void Awake()
        {
            if (!DataMapper) DataMapper = FindObjectOfType<MrPuppetDataMapper>();

            SpawnHipRotation = Hip.localRotation;
            SpawnHeadRotation = Head.localRotation;
        }

        private void LateUpdate()
        {
            // TODO: translate hip (optionally)
            // TODO: clamp hip translate
            // TODO: smooth hip translate

            // apply joint rotation delta onto initial rotation
            Hip.localRotation = SpawnHipRotation * DataMapper.GetJointRotationDelta(MrPuppetDataMapper.Joint.Elbow);
            Head.localRotation = SpawnHeadRotation * DataMapper.GetJointRotationDelta(MrPuppetDataMapper.Joint.Wrist);
        }
    }
}
