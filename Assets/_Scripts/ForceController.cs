﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MrPuppet
{
    public class ForceController : MonoBehaviour
    {
        public float WalkSpeed = 3.41f;

        public float BobFrequency = 0.2f;
        public float BobHeight = 4f;

        private float BobTarget = 0f;

        private Vector3 Position = Vector3.zero;
        private bool IsMoving = false;
        private bool IsUp = false;

        private void Update()
        {
            if (IsMoving)
            {
                if (Input.GetAxisRaw("Horizontal") == 0f)
                {
                    BobTarget = 0f;
                    IsMoving = IsUp = false;
                    CancelInvoke("ToggleBob");
                }
            }
            else
            {
                if (Input.GetAxisRaw("Horizontal") != 0f)
                {
                    IsMoving = true;
                    if (!IsInvoking("ToggleBob")) InvokeRepeating("ToggleBob", 0f, BobFrequency);
                }
            }

            Position = transform.position;
            Position.y = BobTarget;
            transform.position = Position;
        }

        private void ToggleBob()
        {
            IsUp = !IsUp;

            BobTarget = IsUp ? BobHeight : 0f;
        }
    }
}