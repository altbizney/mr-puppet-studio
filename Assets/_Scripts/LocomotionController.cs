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

        public CharacterController controller;

        [ValueDropdown("VectorDirectionValues", HideChildProperties = true)]
        public Vector3 VerticalDirection = Vector3.forward;

        [ValueDropdown("VectorDirectionValues", HideChildProperties = true)]
        public Vector3 HorizontalDirection = Vector3.right;

        public float speed = 5f;

        private void Start()
        {
            if (!controller) controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            Vector3 movementDirection = (VerticalDirection * Input.GetAxis("Vertical") * speed) + (HorizontalDirection * Input.GetAxis("Horizontal") * speed);

            controller.Move(movementDirection * Time.deltaTime);
        }
    }
}