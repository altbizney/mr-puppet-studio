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

        [InfoBox("World axis to translate vertically")]
        [ValueDropdown("VectorDirectionValues", HideChildProperties = true)]
        public Vector3 VerticalDirection = Vector3.forward;

        [InfoBox("World axis to translate horizontally")]
        [ValueDropdown("VectorDirectionValues", HideChildProperties = true)]
        public Vector3 HorizontalDirection = Vector3.right;

        [InfoBox("Corrective rotation to add onto the facing direction")]
        public Vector3 RotationOffset;

        [Tooltip("Modifier for walk speed")]
        public float WalkSpeed = 6f;
        [Tooltip("Angular speed in degrees per sec")]
        public float RotateSpeed = 1080f;

        [Tooltip("SmoothTime for SmoothDamp on Y bob")]
        public float BobSmoothTime = 0.1f;
        [Tooltip("How frequent bob goes between up and down")]
        public float BobFrequency = 0.175f;
        [Tooltip("Height to bob to")]
        public float BobHeight = 2f;

        private bool IsMoving, IsUp = false;
        private float InitialHeight, BobTarget, BobCurrent, BobVelocity = 0f;
        private Vector3 MoveDirection;
        private Quaternion LookDirection;

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
            MoveDirection = (VerticalDirection * Input.GetAxis("Vertical") * WalkSpeed) + (HorizontalDirection * Input.GetAxis("Horizontal") * WalkSpeed);
            Controller.Move(MoveDirection * Time.deltaTime);

            // spring bob height and apply to local position
            BobCurrent = Mathf.SmoothDamp(BobCurrent, BobTarget, ref BobVelocity, BobSmoothTime);
            transform.localPosition = new Vector3(transform.localPosition.x, InitialHeight + BobCurrent, transform.localPosition.z);

            // look in direction were moving
            if (MoveDirection != Vector3.zero)
            {
                LookDirection = Quaternion.Euler(RotationOffset) * Quaternion.LookRotation(MoveDirection);
            }

            // smoothly apply look direction
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, LookDirection, RotateSpeed * Time.deltaTime);
        }

        private void ToggleBob()
        {
            IsUp = !IsUp;
            BobTarget = IsUp ? BobHeight : 0f;
        }
    }
}