using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet
{
    public class DataTransport : MonoBehaviour
    {
        [Serializable]
        private class ChannelMap
        {
            public SkinnedMeshRenderer SkinnedMeshRenderer;

            public int BlendShapeIndex;
            public string Name;
            [Range(0f, 100f)]
            public float Amount;

            public Transform Proxy;
        }

        [ShowInInspector, ReadOnly]
        private List<ChannelMap> Channels = new List<ChannelMap>();

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
                    channel.Name = mr.sharedMesh.GetBlendShapeName(i).Split('.')[1] + "_CHANNEL";
                    channel.Proxy = transform.Find(channel.Name);

                    if (channel.Proxy)
                    {
                        Channels.Add(channel);
                    }
                    else
                    {
                        Debug.LogWarning("Could not find dataTransport channel named: " + channel.Name);
                    }
                }
            }
        }

        private void LateUpdate()
        {
            foreach (var channel in Channels)
            {
                channel.Amount = channel.SkinnedMeshRenderer.GetBlendShapeWeight(channel.BlendShapeIndex);
                channel.Proxy.localPosition = new Vector3(channel.Proxy.localPosition.x, channel.Amount, channel.Proxy.localPosition.z);
            }
        }

        private void OnDrawGizmos()
        {
            if (!DrawGizmos) return;

            foreach (var channel in Channels)
            {
                var start = new Vector3(channel.Proxy.localPosition.x, 0f, channel.Proxy.localPosition.z);
                var end = new Vector3(channel.Proxy.localPosition.x, channel.Amount, channel.Proxy.localPosition.z);

                Gizmos.DrawSphere(start, 0.25f);
                Gizmos.DrawLine(start, end);
                Gizmos.DrawSphere(end, 0.25f);
            }
        }
    }
}
