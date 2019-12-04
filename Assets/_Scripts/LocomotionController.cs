using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet.WIP
{
    public class LocomotionController : MonoBehaviour
    {
        private ValueDropdownList<Vector3> VectorDirectionValues = new ValueDropdownList<Vector3>()
        {
            {"Forward", Vector3.forward },
            {"Back",    Vector3.back    },
            {"Up",      Vector3.up      },
            {"Down",    Vector3.down    },
            {"Right",   Vector3.right   },
            {"Left",    Vector3.left    },
        };

        public CharacterController Controller;

        [ValueDropdown("VectorDirectionValues", HideChildProperties = true)]
        public Vector3 VerticalDirection = Vector3.forward;

        [ValueDropdown("VectorDirectionValues", HideChildProperties = true)]
        public Vector3 HorizontalDirection = Vector3.right;

        public float WalkSpeed = 5f;

        public float BobSmoothTime = 0.1f;
        public float BobFrequency = 0.175f;
        public float BobHeight = 2f;

        private bool IsMoving, IsUp = false;
        private float InitialHeight, BobTarget, BobCurrent, BobVelocity = 0f;

        private void Start()
        {
            if (!Controller) Controller = GetComponent<CharacterController>();

            InitialHeight = transform.localPosition.y;
        }

        private void Update()
        {
            if (IsMoving)
            {
                // Reset bob when joysticks are released
                if (Input.GetAxisRaw("Horizontal") == 0f && Input.GetAxisRaw("Vertical") == 0f)
                {
                    BobTarget = 0f;
                    IsMoving = IsUp = false;
                    CancelInvoke("ToggleBob");
                }
            }
            else
            {
                // initiate bob as soon as joysticks are touched
                if (Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f)
                {
                    IsMoving = true;
                    if (!IsInvoking("ToggleBob")) InvokeRepeating("ToggleBob", 0f, BobFrequency);
                }
            }

            // create direction vector to move on X/Z
            Vector3 movementDirection = (VerticalDirection * Input.GetAxis("Vertical") * WalkSpeed) + (HorizontalDirection * Input.GetAxis("Horizontal") * WalkSpeed);
            Controller.Move(movementDirection * Time.deltaTime);

            // spring bob height and apply to local position
            BobCurrent = Mathf.SmoothDamp(BobCurrent, BobTarget, ref BobVelocity, BobSmoothTime);
            transform.localPosition = new Vector3(transform.localPosition.x, InitialHeight + BobCurrent, transform.localPosition.z);
        }

        private void ToggleBob()
        {
            IsUp = !IsUp;
            BobTarget = IsUp ? BobHeight : 0f;
        }
    }
}