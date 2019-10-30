using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MrPuppet
{
    public class ForceController : MonoBehaviour
    {
        public float WalkSpeed = 3.41f;

        public float BobFrequency = 0.2f;
        public float BobHeight = 4f;

        public float SmoothTime = 0.1f;

        private Vector3 PositionTarget = Vector3.zero;
        private Vector3 PositionCurrent = Vector3.zero;
        private Vector3 PositionVelocity = Vector3.zero;

        private bool IsMoving = false;
        private bool IsUp = false;

        private void Start()
        {
            PositionTarget = PositionCurrent = transform.position;
        }

        private void Update()
        {
            if (IsMoving)
            {
                if (Input.GetAxisRaw("Horizontal") == 0f)
                {
                    PositionTarget.y = 0f;
                    IsMoving = IsUp = false;
                    CancelInvoke("ToggleBob");
                }

                PositionTarget.x += Input.GetAxisRaw("Horizontal") * WalkSpeed;
            }
            else
            {
                if (Input.GetAxisRaw("Horizontal") != 0f)
                {
                    IsMoving = true;
                    if (!IsInvoking("ToggleBob")) InvokeRepeating("ToggleBob", 0f, BobFrequency);
                }
            }

            PositionCurrent = Vector3.SmoothDamp(PositionCurrent, PositionTarget, ref PositionVelocity, SmoothTime);
            transform.position = PositionCurrent;
        }

        private void ToggleBob()
        {
            IsUp = !IsUp;

            PositionTarget.y = IsUp ? BobHeight : 0f;
        }
    }
}