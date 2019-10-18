﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace MrPuppet
{
    public class DataTransport : MonoBehaviour
    {
        [Serializable]
        private class ChannelMap
        {
            public SkinnedMeshRenderer SkinnedMeshRenderer;

            public int BlendShapeIndex;

            public Transform Proxy;
        }

        private List<ChannelMap> Channels;

        public bool DrawGizmos = false;

        private void Awake()
        {
            Channels = new List<ChannelMap>();

            foreach (var mr in transform.root.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                for (var i = 0; i < mr.sharedMesh.blendShapeCount; i++)
                {
                    var channel = new ChannelMap();
                    channel.SkinnedMeshRenderer = mr;
                    channel.BlendShapeIndex = i;
                    channel.Proxy = transform.Find(mr.sharedMesh.GetBlendShapeName(i).Replace(".", "_"));
                    Channels.Add(channel);
                }
            }
        }

        private void LateUpdate()
        {
            foreach (var channel in Channels)
            {
                channel.Proxy.localPosition = new Vector3(channel.Proxy.localPosition.x, channel.SkinnedMeshRenderer.GetBlendShapeWeight(channel.BlendShapeIndex), channel.Proxy.localPosition.z);
            }
        }

        private void OnDrawGizmos()
        {
            if (!DrawGizmos) return;

            foreach (var channel in Channels)
            {
                var start = new Vector3(channel.Proxy.localPosition.x, 0f, channel.Proxy.localPosition.z);
                var end = new Vector3(channel.Proxy.localPosition.x, channel.SkinnedMeshRenderer.GetBlendShapeWeight(channel.BlendShapeIndex), channel.Proxy.localPosition.z);

                Gizmos.DrawSphere(start, 0.25f);
                Gizmos.DrawLine(start, end);
                Gizmos.DrawSphere(end, 0.25f);
            }
        }
    }
}