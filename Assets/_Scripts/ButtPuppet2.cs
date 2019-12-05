using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPuppet.WIP
{
    public class ButtPuppet2 : MonoBehaviour
    {
        private MrPuppetDataMapper DataMapper;

        private void Awake()
        {
            if (!DataMapper) DataMapper = FindObjectOfType<MrPuppetDataMapper>();
        }
    }
}