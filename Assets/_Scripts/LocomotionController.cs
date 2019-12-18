using UnityEngine;
using Sirenix.OdinInspector;

namespace MrPuppet.WIP
{
    public class LocomotionController : MonoBehaviour
    {
        private ValueDropdownList<Vector3> VectorDirectionValues = new ValueDropdownList<Vector3>()
        { { "Forward", Vector3.forward }, { "Back", Vector3.back }, { "Up", Vector3.up }, { "Down", Vector3.down }, { "Right", Vector3.right }, { "Left", Vector3.left },
        };

        public bool UseRigidbody = false;

        public CharacterController Controller;
        public Rigidbody rb;

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

        public bool EnableRotation = true;
        [Tooltip("Angular speed in degrees/sec"), ShowIf("EnableRotation")]
        public float RotateSpeed = 1080f;

        [Title("Bob")]
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

        private void Awake()
        {
            if (!UseRigidbody && !Controller) Controller = GetComponent<CharacterController>();
            if (UseRigidbody && !rb) rb = GetComponent<Rigidbody>();

            InitialHeight = transform.localPosition.y;
        }

        private void Update()
        {
            // control bob trigger
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

            // calculate direction vector to move on X/Z
            MoveDirection = (VerticalDirection * Input.GetAxis("Vertical") * WalkSpeed) + (HorizontalDirection * Input.GetAxis("Horizontal") * WalkSpeed);

            // calculate look direction
            if (EnableRotation && MoveDirection != Vector3.zero)
            {
                LookDirection = Quaternion.Euler(RotationOffset) * Quaternion.LookRotation(MoveDirection);
            }

            // spring bob height
            BobCurrent = Mathf.SmoothDamp(BobCurrent, BobTarget, ref BobVelocity, BobSmoothTime);

            if (UseRigidbody)
            {
                // apply bob instantly on rb position
                rb.position = new Vector3(transform.localPosition.x, InitialHeight + BobCurrent, transform.localPosition.z);
            }
            else
            {
                // apply bob on transform localPosition
                transform.localPosition = new Vector3(transform.localPosition.x, InitialHeight + BobCurrent, transform.localPosition.z);

                // move controller
                Controller.Move(MoveDirection * Time.deltaTime);

                // smoothly apply look direction
                transform.localRotation = Quaternion.RotateTowards(transform.localRotation, LookDirection, RotateSpeed * Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            // only rigidbody-based movement below
            if (!UseRigidbody) return;

            // add move direction onto current world position
            rb.MovePosition(transform.position + (MoveDirection * Time.fixedDeltaTime));

            // set move rotation to look direction
            if (EnableRotation) rb.MoveRotation(LookDirection.normalized);
        }

        private void ToggleBob()
        {
            IsUp = !IsUp;
            BobTarget = IsUp ? BobHeight : 0f;
        }
    }
}
